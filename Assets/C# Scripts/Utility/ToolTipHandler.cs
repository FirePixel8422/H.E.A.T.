using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    private void Awake() => Instance = this;

    [Header("Tooltip UI")]
    public GameObject tooltipPrefab;

    [Header("Tooltip Data")]
    [SerializeField] private ToolTipWord[] toolTipWords;

    private Dictionary<string, string> wordToTooltip;
    private TextMeshProUGUI[] registeredTextGuis;
    private GameObject activeTooltip;

    private string lastTooltipText;
    private TextMeshProUGUI lastTextObject;

    [SerializeField] private float toolTipHeight = 40f;
    private bool updated;


    public void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    public void OnDisable() => UpdateScheduler.UnRegisterUpdate(OnUpdate);

    private void Start()
    {
        wordToTooltip = new Dictionary<string, string>();
        foreach (var word in toolTipWords)
            wordToTooltip[word.word.ToLower()] = word.toolTip;

        registeredTextGuis = GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var textGui in registeredTextGuis)
        {
            string newText = textGui.text;
            foreach (var word in toolTipWords)
            {
                string coloredWord = $"<color=#{ColorUtility.ToHtmlStringRGB(word.wordColor)}>{word.word}</color>";
                newText = newText.Replace(word.word, coloredWord);
            }
            textGui.text = newText;
        }

        activeTooltip = Instantiate(tooltipPrefab, transform.root);
        activeTooltip.SetActive(false);
    }

    private void OnUpdate()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        foreach (var tmpText in registeredTextGuis)
        {
            if (tmpText == null) continue;

            int wordIndex = TMP_TextUtilities.FindIntersectingWord(tmpText, mousePos, null);
            if (wordIndex == -1) continue;

            TMP_WordInfo wordInfo = tmpText.textInfo.wordInfo[wordIndex];
            string hoveredWord = wordInfo.GetWord().ToLower();

            if (wordToTooltip.TryGetValue(hoveredWord, out string tooltipText))
            {
                // Skip if same word on same TextMeshPro as last frame
                if (tooltipText == lastTooltipText && tmpText == lastTextObject)
                {
                    if (updated) return;
                    updated = true;
                }
                else
                {
                    updated = false;
                    lastTooltipText = tooltipText;
                    lastTextObject = tmpText;
                }

                // Get word bounds
                GetWordWorldBounds(tmpText, wordInfo, out Vector3 bl, out Vector3 tr);

                Vector3 wordMid = (bl + tr) * 0.5f;
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, wordMid);

                float hoveredWordHeight = Mathf.Abs(tr.y - bl.y);
                float hoveredWordWidth = Mathf.Abs(tr.x - bl.x);

                // Tooltip RectTransform and text
                RectTransform tooltipRect = activeTooltip.GetComponent<RectTransform>();
                TextMeshProUGUI tooltipTextComponent = activeTooltip.GetComponentInChildren<TextMeshProUGUI>();

                // Set text + enable autosize
                tooltipTextComponent.enableAutoSizing = true;
                tooltipTextComponent.text = tooltipText;

                // Set an initial max width limit to constrain autosize (here half screen width)
                tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenSize.x * 0.5f);
                tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, toolTipHeight);

                // Force mesh update for accurate bounds
                tooltipTextComponent.ForceMeshUpdate();

                // Now get rendered text width from bounds
                float actualWidth = tooltipTextComponent.textBounds.size.x;

                // Apply final size to tooltip rect
                tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actualWidth + 20);
                tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, toolTipHeight);

                // Disable autosize now to lock it
                tooltipTextComponent.enableAutoSizing = false;

                // Offset tooltip above word
                Vector3 offset = new Vector3(actualWidth * 0.5f - hoveredWordWidth * 0.5f, hoveredWordHeight + toolTipHeight * 0.5f, 0);
                Vector3 finalPos = screenPos + offset;

                // Clamp X within screen
                finalPos.x = Mathf.Clamp(finalPos.x, actualWidth * 0.5f + 15, screenSize.x - actualWidth * 0.5f - 15);
                // Clamp Y within screen
                finalPos.y = Mathf.Clamp(finalPos.y, toolTipHeight * 0.5f, screenSize.y - toolTipHeight * 0.5f);

                activeTooltip.transform.position = finalPos;
                activeTooltip.SetActiveSmart(true);
                return;
            }
        }

        // No word hovered — hide tooltip
        activeTooltip.SetActiveSmart(false);
        lastTooltipText = null;
        lastTextObject = null;
    }


    private void GetWordWorldBounds(TextMeshProUGUI tmpText, TMP_WordInfo wordInfo, out Vector3 bl, out Vector3 tr)
    {
        TMP_TextInfo textInfo = tmpText.textInfo;
        bl = Vector3.positiveInfinity;
        tr = Vector3.negativeInfinity;

        for (int i = 0; i < wordInfo.characterCount; i++)
        {
            int charIndex = wordInfo.firstCharacterIndex + i;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
            if (!charInfo.isVisible) continue;

            Vector3 charBL = tmpText.transform.TransformPoint(charInfo.bottomLeft);
            Vector3 charTR = tmpText.transform.TransformPoint(charInfo.topRight);

            bl = Vector3.Min(bl, charBL);
            tr = Vector3.Max(tr, charTR);
        }
    }
}
