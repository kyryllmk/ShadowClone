using ShadowClone.Clone;
using ShadowClone.Core;
using ShadowClone.Gameplay;
using ShadowClone.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowClone.Presentation
{
    public class PresentationFeedbackBootstrap : MonoBehaviour
    {
        private enum WaveShape
        {
            Sine,
            Triangle,
            Square,
            SoftNoise
        }

        private const string BootstrapObjectName = "PresentationFeedbackBootstrap";
        private static PresentationFeedbackBootstrap instance;

        private AudioSource sfxSource;
        private AudioSource ambienceSource;
        private AudioClip jumpClip;
        private AudioClip landClip;
        private AudioClip recordStartClip;
        private AudioClip recordStopClip;
        private AudioClip replayClip;
        private AudioClip buttonClip;
        private AudioClip doorOpenClip;
        private AudioClip doorCloseClip;
        private AudioClip hazardClip;
        private AudioClip completeClip;
        private AudioClip starClip;
        private AudioClip menuClip;
        private AudioClip pauseClip;
        private AudioClip restartClip;
        private AudioClip levelStartClip;
        private AudioClip menuAmbienceClip;
        private AudioClip tutorialAmbienceClip;
        private AudioClip buttonDoorAmbienceClip;
        private AudioClip hazardTimingAmbienceClip;
        private AudioClip finalAmbienceClip;
        private float beatClockStartTime;
        private float beatInterval = 0.625f;
        private float beatPhase01;
        private float beatPulse01;
        private int currentBeatIndex = -1;

        public static float CurrentBpm => instance != null ? instance.GetCurrentBpm() : 96f;
        public static float BeatPhase01 => instance != null ? instance.beatPhase01 : 0f;
        public static float BeatPulse01 => instance != null ? instance.beatPulse01 : 0f;
        public static int CurrentBeatIndex => instance != null ? instance.currentBeatIndex : -1;

        private PlayerController playerController;
        private RecordingController recordingController;
        private CloneReplayController cloneReplayController;
        private RoomResetManager roomResetManager;
        private PuzzleButton[] buttons = System.Array.Empty<PuzzleButton>();
        private DoorController[] doors = System.Array.Empty<DoorController>();
        private Hazard[] hazards = System.Array.Empty<Hazard>();
        private GoalZone[] goals = System.Array.Empty<GoalZone>();
        private float lastHazardSoundTime = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(BootstrapObjectName);
            bootstrapObject.AddComponent<PresentationFeedbackBootstrap>();
        }

        public static void PlayMenuConfirm()
        {
            instance?.PlayClip(instance.menuClip, 0.3f);
        }

        public static void PlayPauseToggle()
        {
            instance?.PlayClip(instance.pauseClip, 0.22f);
        }

        public static void PlayRestartLevel()
        {
            instance?.PlayClip(instance.restartClip, 0.24f);
        }

        public static void PlayLevelStart()
        {
            instance?.PlayClip(instance.levelStartClip, 0.22f);
        }

        public static void PlayStarCollected()
        {
            instance?.PlayClip(instance.starClip, 0.32f);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.ignoreListenerPause = true;

            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.ignoreListenerPause = true;
            ambienceSource.volume = 0.42f;

            BuildClips();
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void Update()
        {
            UpdateBeatClock();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }

            UnbindSceneFeedback();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnbindSceneFeedback();

            if (scene.name == SceneRegistry.MainMenu)
            {
                ConfigureBeatClock(88f);
                PlayAmbience(menuAmbienceClip, 0.32f);
                return;
            }

            ConfigureBeatClock(GetLevelBpm(scene.name));
            PlayAmbience(GetLevelAmbience(scene.name), 0.75f);
            PlayLevelStart();

            playerController = FindObjectOfType<PlayerController>();
            roomResetManager = FindObjectOfType<RoomResetManager>();
            recordingController = FindObjectOfType<RecordingController>();
            cloneReplayController = FindObjectOfType<CloneReplayController>();
            buttons = FindObjectsOfType<PuzzleButton>();
            doors = FindObjectsOfType<DoorController>();
            hazards = FindObjectsOfType<Hazard>();
            goals = FindObjectsOfType<GoalZone>();

            if (playerController != null)
            {
                playerController.Jumped += HandleJumped;
                playerController.Landed += HandleLanded;
            }

            if (roomResetManager != null)
            {
                roomResetManager.ResetCompleted += HandleResetCompleted;
            }

            if (recordingController != null)
            {
                recordingController.RecordingStateChanged += HandleRecordingStateChanged;
                recordingController.RecordingFinished += HandleRecordingFinished;
            }

            if (cloneReplayController != null)
            {
                cloneReplayController.ReplayStarted += HandleReplayStarted;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].PressedStateChanged += HandleButtonPressedStateChanged;
            }

            for (int i = 0; i < doors.Length; i++)
            {
                doors[i].StateChanged += HandleDoorStateChanged;
            }

            for (int i = 0; i < hazards.Length; i++)
            {
                hazards[i].Triggered += HandleHazardTriggered;
            }

            for (int i = 0; i < goals.Length; i++)
            {
                goals[i].Completed += HandleGoalCompleted;
            }
        }

        private void UnbindSceneFeedback()
        {
            if (playerController != null)
            {
                playerController.Jumped -= HandleJumped;
                playerController.Landed -= HandleLanded;
                playerController = null;
            }

            if (roomResetManager != null)
            {
                roomResetManager.ResetCompleted -= HandleResetCompleted;
                roomResetManager = null;
            }

            if (recordingController != null)
            {
                recordingController.RecordingStateChanged -= HandleRecordingStateChanged;
                recordingController.RecordingFinished -= HandleRecordingFinished;
                recordingController = null;
            }

            if (cloneReplayController != null)
            {
                cloneReplayController.ReplayStarted -= HandleReplayStarted;
                cloneReplayController = null;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].PressedStateChanged -= HandleButtonPressedStateChanged;
                }
            }

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null)
                {
                    doors[i].StateChanged -= HandleDoorStateChanged;
                }
            }

            for (int i = 0; i < hazards.Length; i++)
            {
                if (hazards[i] != null)
                {
                    hazards[i].Triggered -= HandleHazardTriggered;
                }
            }

            for (int i = 0; i < goals.Length; i++)
            {
                if (goals[i] != null)
                {
                    goals[i].Completed -= HandleGoalCompleted;
                }
            }

            buttons = System.Array.Empty<PuzzleButton>();
            doors = System.Array.Empty<DoorController>();
            hazards = System.Array.Empty<Hazard>();
            goals = System.Array.Empty<GoalZone>();
        }

        private void HandleJumped()
        {
            PlayClip(jumpClip, 0.26f);
        }

        private void HandleLanded()
        {
            PlayClip(landClip, 0.22f);
        }

        private void HandleRecordingStateChanged(bool isRecording)
        {
            if (isRecording)
            {
                PlayClip(recordStartClip, 0.24f);
            }
        }

        private void HandleRecordingFinished(RecordingClip clip)
        {
            PlayClip(recordStopClip, 0.24f);
        }

        private void HandleReplayStarted()
        {
            PlayClip(replayClip, 0.24f);
        }

        private void HandleButtonPressedStateChanged(bool isPressed)
        {
            if (isPressed)
            {
                PlayClip(buttonClip, 0.22f);
            }
        }

        private void HandleDoorStateChanged(bool isOpen)
        {
            PlayClip(isOpen ? doorOpenClip : doorCloseClip, 0.2f);
        }

        private void HandleHazardTriggered()
        {
            PlayHazardSound();
        }

        private void HandleResetCompleted(string reason)
        {
            if (reason != "Fall reset" && reason != "Out of bounds")
            {
                return;
            }

            PlayHazardSound();
        }

        private void HandleGoalCompleted()
        {
            PlayClip(completeClip, 0.28f);
        }

        private void PlayHazardSound()
        {
            if (Time.unscaledTime - lastHazardSoundTime < 0.05f)
            {
                return;
            }

            lastHazardSoundTime = Time.unscaledTime;
            PlayClip(hazardClip, 0.28f);
        }

        private void PlayClip(AudioClip clip, float volumeScale)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, volumeScale);
        }

        private void PlayAmbience(AudioClip clip, float volume)
        {
            if (clip == null || ambienceSource == null)
            {
                return;
            }

            if (ambienceSource.clip == clip && ambienceSource.isPlaying)
            {
                ambienceSource.volume = volume;
                return;
            }

            ambienceSource.Stop();
            ambienceSource.clip = clip;
            ambienceSource.volume = volume;
            ambienceSource.Play();
        }

        private void BuildClips()
        {
            jumpClip = BuildSweepClip("jump", WaveShape.Triangle, 380f, 0.12f, 0.18f, 760f, 0.01f, 0f);
            landClip = BuildSweepClip("land", WaveShape.Sine, 210f, 0.11f, 0.22f, 120f, 0f, 0.05f);
            recordStartClip = BuildSweepClip("recordStart", WaveShape.Square, 640f, 0.13f, 0.16f, 920f, 0.02f, 0f);
            recordStopClip = BuildSweepClip("recordStop", WaveShape.Triangle, 920f, 0.13f, 0.16f, 420f, 0.015f, 0f);
            replayClip = BuildReplayClip("replay");
            buttonClip = BuildSweepClip("button", WaveShape.Square, 280f, 0.1f, 0.12f, 240f, 0f, 0f);
            doorOpenClip = BuildSweepClip("doorOpen", WaveShape.Sine, 150f, 0.24f, 0.18f, 340f, 0.01f, 0.02f);
            doorCloseClip = BuildSweepClip("doorClose", WaveShape.Sine, 300f, 0.2f, 0.16f, 130f, 0.005f, 0.03f);
            hazardClip = BuildNoiseBurstClip("hazard", 0.16f, 0.22f, 220f);
            completeClip = BuildChordClip("complete", 0.42f, 0.15f, new[] { 392f, 523.25f, 659.25f });
            starClip = BuildChordClip("star", 0.18f, 0.15f, new[] { 659.25f, 783.99f, 1046.5f });
            menuClip = BuildChordClip("menu", 0.16f, 0.12f, new[] { 392f, 523.25f });
            pauseClip = BuildSweepClip("pause", WaveShape.Triangle, 540f, 0.11f, 0.13f, 320f, 0f, 0f);
            restartClip = BuildSweepClip("restart", WaveShape.Square, 300f, 0.15f, 0.14f, 620f, 0.01f, 0.01f);
            levelStartClip = BuildChordClip("levelStart", 0.22f, 0.11f, new[] { 349.23f, 440f, 523.25f });
            menuAmbienceClip = BuildAmbientLoop("menuAmbience", 5.5f, 95f, 165f, 0.035f, 0.02f);
            tutorialAmbienceClip = BuildRhythmLoop("level01_96bpm", 96f, 0.16f, 16, 0);
            buttonDoorAmbienceClip = BuildRhythmLoop("level02_108bpm", 108f, 0.17f, 16, 1);
            hazardTimingAmbienceClip = BuildRhythmLoop("level03_124bpm", 124f, 0.18f, 16, 2);
            finalAmbienceClip = BuildRhythmLoop("level04_136bpm", 136f, 0.19f, 16, 3);
        }

        private AudioClip BuildSweepClip(string clipName, WaveShape shape, float startFrequency, float duration, float amplitude, float endFrequency, float vibratoAmount, float noiseMix)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float phase = 0f;
            Random.State previousRandomState = Random.state;
            Random.InitState(clipName.GetHashCode());

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                frequency += Mathf.Sin(t * Mathf.PI * 8f) * startFrequency * vibratoAmount;
                phase += 2f * Mathf.PI * frequency / sampleRate;

                float attack = Mathf.Clamp01(t / 0.15f);
                float release = Mathf.Clamp01((1f - t) / 0.35f);
                float envelope = attack * release;

                float waveform = EvaluateWave(shape, phase);
                float noise = (Random.value * 2f - 1f) * noiseMix;
                samples[i] = Mathf.Clamp((waveform + noise) * amplitude * envelope, -1f, 1f);
            }

            Random.state = previousRandomState;
            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip BuildNoiseBurstClip(string clipName, float duration, float amplitude, float filterFrequency)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float lastValue = 0f;
            Random.State previousRandomState = Random.state;
            Random.InitState(clipName.GetHashCode());

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float noise = Random.value * 2f - 1f;
                float cutoff = Mathf.Lerp(filterFrequency * 2.2f, filterFrequency * 0.6f, t);
                float lerp = Mathf.Clamp01(cutoff / sampleRate * 18f);
                lastValue = Mathf.Lerp(lastValue, noise, lerp);
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(1f - Mathf.Abs(t - 0.5f) * 2f));
                samples[i] = lastValue * amplitude * envelope;
            }

            Random.state = previousRandomState;
            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip BuildChordClip(string clipName, float duration, float amplitude, float[] notes)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float[] phases = new float[notes.Length];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));
                float value = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    phases[n] += 2f * Mathf.PI * notes[n] / sampleRate;
                    value += Mathf.Sin(phases[n]) * (1f / notes.Length);
                }

                samples[i] = value * amplitude * envelope;
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip BuildReplayClip(string clipName)
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float phaseA = 0f;
            float phaseB = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freqA = Mathf.Lerp(260f, 540f, t);
                float freqB = Mathf.Lerp(390f, 780f, t);
                phaseA += 2f * Mathf.PI * freqA / sampleRate;
                phaseB += 2f * Mathf.PI * freqB / sampleRate;
                float envelope = Mathf.Clamp01((1f - t) * 1.2f);
                float tone = EvaluateWave(WaveShape.Square, phaseA) * 0.6f + EvaluateWave(WaveShape.Triangle, phaseB) * 0.4f;
                samples[i] = tone * 0.18f * envelope;
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip BuildAmbientLoop(string clipName, float duration, float baseFrequencyA, float baseFrequencyB, float amplitude, float pulseAmplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float phaseA = 0f;
            float phaseB = 0f;
            float phasePulse = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float slowMotion = Mathf.Sin(t * Mathf.PI * 2f);
                phaseA += 2f * Mathf.PI * (baseFrequencyA + slowMotion * 2f) / sampleRate;
                phaseB += 2f * Mathf.PI * (baseFrequencyB + slowMotion * 3f) / sampleRate;
                phasePulse += 2f * Mathf.PI * 0.7f / sampleRate;

                float drone = Mathf.Sin(phaseA) * 0.65f + Mathf.Sin(phaseB) * 0.35f;
                float pulse = Mathf.Sin(phasePulse) * pulseAmplitude;
                float fade = Mathf.Sin(Mathf.PI * t);
                samples[i] = (drone * amplitude + pulse) * (0.5f + fade * 0.5f);
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip BuildRhythmLoop(string clipName, float bpm, float amplitude, int beatCount, int intensity)
        {
            const int sampleRate = 44100;
            float secondsPerBeat = 60f / bpm;
            int sampleCount = Mathf.CeilToInt(sampleRate * secondsPerBeat * beatCount);
            float[] samples = new float[sampleCount];
            float phaseBass = 0f;
            float phasePadA = 0f;
            float phasePadB = 0f;
            Random.State previousRandomState = Random.state;
            Random.InitState(clipName.GetHashCode());

            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                float beatPosition = time / secondsPerBeat;
                int beat = Mathf.FloorToInt(beatPosition);
                float beatPhase = beatPosition - beat;
                float eighthPhase = (beatPosition * 2f) - Mathf.Floor(beatPosition * 2f);

                float kick = Mathf.Exp(-beatPhase * Mathf.Lerp(16f, 22f, intensity / 3f)) * Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(52f, 72f, 1f - beatPhase) * time);
                float hat = Mathf.Exp(-eighthPhase * 32f) * (Random.value * 2f - 1f) * (0.18f + intensity * 0.035f);
                float accent = beat % 4 == 0 ? 1f : 0.45f;

                float bassFrequency = 82.41f * (1f + ((beat + intensity) % 4 == 2 ? 0.5f : 0f));
                phaseBass += 2f * Mathf.PI * bassFrequency / sampleRate;
                phasePadA += 2f * Mathf.PI * Mathf.Lerp(138f, 164f, intensity / 3f) / sampleRate;
                phasePadB += 2f * Mathf.PI * Mathf.Lerp(207f, 246f, intensity / 3f) / sampleRate;

                float bassGate = beatPhase < 0.48f ? 1f - beatPhase * 1.55f : 0f;
                float bass = EvaluateWave(WaveShape.Triangle, phaseBass) * bassGate * 0.42f;
                float pad = (Mathf.Sin(phasePadA) * 0.55f + Mathf.Sin(phasePadB) * 0.45f) * 0.16f;
                float syncopation = ((beat + intensity) % 8 == 6 && beatPhase < 0.5f) ? Mathf.Sin(phaseBass * 2f) * 0.14f : 0f;
                float envelope = 0.85f + Mathf.Sin((time / (secondsPerBeat * beatCount)) * Mathf.PI * 2f) * 0.05f;

                samples[i] = Mathf.Clamp((kick * 0.34f * accent + hat + bass + pad + syncopation) * amplitude * envelope, -1f, 1f);
            }

            Random.state = previousRandomState;
            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip GetLevelAmbience(string sceneName)
        {
            switch (sceneName)
            {
                case SceneRegistry.Tutorial:
                    return tutorialAmbienceClip;
                case SceneRegistry.ButtonDoor:
                    return buttonDoorAmbienceClip;
                case SceneRegistry.HazardTiming:
                    return hazardTimingAmbienceClip;
                case SceneRegistry.Final:
                    return finalAmbienceClip;
                default:
                    return tutorialAmbienceClip;
            }
        }

        private void ConfigureBeatClock(float bpm)
        {
            beatInterval = Mathf.Max(0.001f, 60f / bpm);
            beatClockStartTime = Time.unscaledTime;
            currentBeatIndex = -1;
            UpdateBeatClock();
        }

        private void UpdateBeatClock()
        {
            float elapsed = Mathf.Max(0f, Time.unscaledTime - beatClockStartTime);
            float beatPosition = elapsed / Mathf.Max(0.001f, beatInterval);
            currentBeatIndex = Mathf.FloorToInt(beatPosition);
            beatPhase01 = beatPosition - currentBeatIndex;
            beatPulse01 = Mathf.Pow(1f - beatPhase01, 3.2f);
        }

        private float GetCurrentBpm()
        {
            return 60f / Mathf.Max(0.001f, beatInterval);
        }

        private static float GetLevelBpm(string sceneName)
        {
            switch (sceneName)
            {
                case SceneRegistry.Tutorial:
                    return 96f;
                case SceneRegistry.ButtonDoor:
                    return 108f;
                case SceneRegistry.HazardTiming:
                    return 124f;
                case SceneRegistry.Final:
                    return 136f;
                default:
                    return 96f;
            }
        }

        private static float EvaluateWave(WaveShape shape, float phase)
        {
            switch (shape)
            {
                case WaveShape.Triangle:
                    return Mathf.PingPong(phase / Mathf.PI, 2f) - 1f;
                case WaveShape.Square:
                    return Mathf.Sign(Mathf.Sin(phase));
                case WaveShape.SoftNoise:
                    return (Mathf.PerlinNoise(phase, 0f) * 2f) - 1f;
                default:
                    return Mathf.Sin(phase);
            }
        }

        private static AudioClip CreateClip(string clipName, float[] samples, int sampleRate)
        {
            AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
