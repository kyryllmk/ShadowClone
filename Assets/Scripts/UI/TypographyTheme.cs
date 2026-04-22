using TMPro;
using UnityEngine;

namespace ShadowClone.UI
{
    public static class TypographyTheme
    {
        private static TMP_FontAsset primaryFont;
        private static TMP_FontAsset secondaryFont;

        public static TMP_FontAsset PrimaryFont => primaryFont != null ? primaryFont : (primaryFont = LoadPrimaryFont());
        public static TMP_FontAsset SecondaryFont => secondaryFont != null ? secondaryFont : (secondaryFont = LoadSecondaryFont());

        public static void ApplyTitle(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            text.font = PrimaryFont;
            text.fontSize = Mathf.Max(text.fontSize, 64f);
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 8f;
            text.wordSpacing = 0f;
            text.text = NormalizeToken(text.text);
        }

        public static void ApplyButton(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            text.font = PrimaryFont;
            text.fontSize = Mathf.Max(text.fontSize, 28f);
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 4f;
            text.text = NormalizeToken(text.text);
        }

        public static void ApplyHud(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            text.font = SecondaryFont;
            text.fontSize = Mathf.Max(text.fontSize, 22f);
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 2f;
            text.text = NormalizeToken(text.text);
        }

        public static void ApplyState(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            text.font = SecondaryFont;
            text.fontSize = Mathf.Max(text.fontSize, 26f);
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 3f;
            text.text = NormalizeToken(text.text);
        }

        public static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim().ToUpperInvariant();
        }

        private static TMP_FontAsset LoadPrimaryFont()
        {
            TMP_FontAsset preferred =
                Resources.Load<TMP_FontAsset>("Fonts & Materials/Oxanium SDF") ??
                Resources.Load<TMP_FontAsset>("Fonts & Materials/Oxanium-Regular SDF") ??
                Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");

            return preferred != null ? preferred : TMP_Settings.defaultFontAsset;
        }

        private static TMP_FontAsset LoadSecondaryFont()
        {
            TMP_FontAsset preferred =
                Resources.Load<TMP_FontAsset>("Fonts & Materials/Rajdhani SDF") ??
                Resources.Load<TMP_FontAsset>("Fonts & Materials/Exo 2 SDF") ??
                Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald Bold SDF");

            return preferred != null ? preferred : TMP_Settings.defaultFontAsset;
        }
    }
}
