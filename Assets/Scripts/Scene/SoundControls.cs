using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// Plain sound rows for the Settings and Pause panels. The scene's slider and toggles keep
    /// their wiring; only their look changes. Colors come from the panel's themed heading, so
    /// every theme fits without its own art.
    /// </summary>
    public static class SoundControls
    {
        private const string SoundPanelName = "SoundPanel";
        private const string HolderName = "Holder";
        private const string HeadingName = "T_SoundSettings";
        private const string VolumeName = "T_Volume";
        private const string MusicName = "T_BG_Music";
        private const string EffectsName = "T_SFX";

        private const float RowWidth = 180f;
        private const float RowHeight = 26f;
        private const float LabelFontSize = 17f;
        private const float VolumeY = 54f;
        private const float SliderOffsetY = -28f;
        private const float MusicY = -26f;
        private const float EffectsY = -64f;
        private const float TrackHeight = 6f;
        private const float HandleSize = 20f;
        private const float OffAlpha = 0.25f;
        private const int CircleSize = 64;

        private static Sprite _pill;

        public static void RestyleAll()
        {
            foreach (Image panel in Resources.FindObjectsOfTypeAll<Image>())
                if (panel.gameObject.scene.IsValid() && panel.name == SoundPanelName)
                    Restyle(panel);
        }

        private static void Restyle(Image panel)
        {
            Transform holder = panel.transform.Find(HolderName);
            Transform heading = holder.Find(HeadingName);
            Color accent = heading.GetComponent<TMP_Text>().color;
            heading.gameObject.SetActive(false);
            panel.enabled = false;
            Place((RectTransform)holder, Vector2.zero, ((RectTransform)holder).rect.size);

            Transform volume = holder.Find(VolumeName);
            StyleLabel(volume, VolumeY);
            StyleSlider(volume.GetComponentInChildren<Slider>(true), accent);
            StyleSwitch(holder.Find(MusicName), MusicY, accent);
            StyleSwitch(holder.Find(EffectsName), EffectsY, accent);
        }

        private static void StyleLabel(Transform row, float y)
        {
            Place((RectTransform)row, new Vector2(0f, y), new Vector2(RowWidth, RowHeight));
            row.localScale = Vector3.one;

            TMP_Text label = row.GetComponent<TMP_Text>();
            label.enableAutoSizing = false;
            label.fontSize = LabelFontSize;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.margin = Vector4.zero;
        }

        private static void StyleSlider(Slider slider, Color accent)
        {
            slider.transition = Selectable.Transition.None;
            RectTransform rect = (RectTransform)slider.transform;
            rect.localScale = Vector3.one;
            Place(rect, new Vector2(0f, SliderOffsetY), new Vector2(RowWidth, HandleSize + 4f));

            Image track = rect.Find("Background").GetComponent<Image>();
            Fit(track, new Color(1f, 1f, 1f, OffAlpha), TrackHeight);
            CenterTrack((RectTransform)track.transform);

            RectTransform fillArea = (RectTransform)rect.Find("Fill Area");
            CenterTrack(fillArea);
            fillArea.offsetMin = new Vector2(0f, fillArea.offsetMin.y);
            fillArea.offsetMax = new Vector2(0f, fillArea.offsetMax.y);
            Fit(slider.fillRect.GetComponent<Image>(), accent, TrackHeight);

            slider.handleRect.sizeDelta = new Vector2(HandleSize, 0f);
            RectTransform handleArea = (RectTransform)slider.handleRect.parent;
            handleArea.offsetMin = new Vector2(HandleSize * 0.5f, handleArea.offsetMin.y);
            handleArea.offsetMax = new Vector2(-HandleSize * 0.5f, handleArea.offsetMax.y);
            Fit(slider.handleRect.GetComponent<Image>(), Color.white, HandleSize);
        }

        private static void StyleSwitch(Transform row, float y, Color accent)
        {
            StyleLabel(row, y);
            Toggle toggle = row.GetComponentInChildren<Toggle>(true);
            toggle.gameObject.AddComponent<ToggleSwitch>().Build(toggle, RowWidth, accent, OffAlpha);
        }

        private static void CenterTrack(RectTransform rect)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, 0.5f);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 0.5f);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, TrackHeight);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
        }

        /// <summary>Makes the image a pill of the given height, with fully round ends.</summary>
        public static void Fit(Image image, Color color, float height)
        {
            image.sprite = Pill;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = CircleSize / height;
            image.color = color;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        // Drawn here so no theme has to supply it. Slicing stretches the circle into any pill.
        private static Sprite Pill
        {
            get
            {
                if (_pill != null) return _pill;

                Texture2D texture = new Texture2D(CircleSize, CircleSize, TextureFormat.RGBA32, false);
                float radius = CircleSize * 0.5f;
                for (int y = 0; y < CircleSize; y++)
                {
                    for (int x = 0; x < CircleSize; x++)
                    {
                        float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Vector2.one * radius);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - distance)));
                    }
                }
                texture.Apply();

                Rect all = new Rect(0f, 0f, CircleSize, CircleSize);
                _pill = Sprite.Create(texture, all, Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, Vector4.one * (radius - 1f));
                return _pill;
            }
        }
    }
}
