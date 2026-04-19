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
        private AudioClip menuClip;
        private AudioClip pauseClip;
        private AudioClip restartClip;
        private AudioClip levelStartClip;
        private AudioClip menuAmbienceClip;
        private AudioClip tutorialAmbienceClip;
        private AudioClip buttonDoorAmbienceClip;
        private AudioClip hazardTimingAmbienceClip;
        private AudioClip finalAmbienceClip;

        private PlayerController playerController;
        private RecordingController recordingController;
        private CloneReplayController cloneReplayController;
        private PuzzleButton[] buttons = System.Array.Empty<PuzzleButton>();
        private DoorController[] doors = System.Array.Empty<DoorController>();
        private Hazard[] hazards = System.Array.Empty<Hazard>();
        private GoalZone[] goals = System.Array.Empty<GoalZone>();

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
                PlayAmbience(menuAmbienceClip, 0.32f);
                return;
            }

            PlayAmbience(GetLevelAmbience(scene.name), 0.3f);
            PlayLevelStart();

            playerController = FindObjectOfType<PlayerController>();
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
            PlayClip(hazardClip, 0.28f);
        }

        private void HandleGoalCompleted()
        {
            PlayClip(completeClip, 0.28f);
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
            menuClip = BuildChordClip("menu", 0.16f, 0.12f, new[] { 392f, 523.25f });
            pauseClip = BuildSweepClip("pause", WaveShape.Triangle, 540f, 0.11f, 0.13f, 320f, 0f, 0f);
            restartClip = BuildSweepClip("restart", WaveShape.Square, 300f, 0.15f, 0.14f, 620f, 0.01f, 0.01f);
            levelStartClip = BuildChordClip("levelStart", 0.22f, 0.11f, new[] { 349.23f, 440f, 523.25f });
            menuAmbienceClip = BuildAmbientLoop("menuAmbience", 5.5f, 95f, 165f, 0.035f, 0.02f);
            tutorialAmbienceClip = BuildAmbientLoop("tutorialAmbience", 6.0f, 78f, 128f, 0.028f, 0.01f);
            buttonDoorAmbienceClip = BuildAmbientLoop("buttonDoorAmbience", 6.2f, 70f, 110f, 0.032f, 0.012f);
            hazardTimingAmbienceClip = BuildAmbientLoop("hazardTimingAmbience", 6.4f, 62f, 98f, 0.035f, 0.016f);
            finalAmbienceClip = BuildAmbientLoop("finalAmbience", 6.8f, 58f, 92f, 0.038f, 0.018f);
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
