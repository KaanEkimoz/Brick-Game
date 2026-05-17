using Board;
using TMPro;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Drives the "Lines" HUD counter — the total number of lines the player has cleared in
    /// the current run. Listens to BoardController.OnTotalClearedLinesChanged (BoardController
    /// fires this every time a line is removed) and to BoardController.ResetTotalClearedLines
    /// indirectly via scene reload, plus a defensive zero on Start.
    /// </summary>
    public class LinesController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _linesText;

        private void OnEnable()
        {
            BoardController.OnTotalClearedLinesChanged += UpdateLinesText;
        }

        private void OnDisable()
        {
            BoardController.OnTotalClearedLinesChanged -= UpdateLinesText;
        }

        private void Start()
        {
            UpdateLinesText(0);
        }

        private void UpdateLinesText(int totalClearedLines)
        {
            if (_linesText == null) return;
            _linesText.text = "Lines\n" + totalClearedLines;
        }
    }
}
