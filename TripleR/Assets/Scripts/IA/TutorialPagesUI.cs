using TMPro;
using UnityEngine;

public class TutorialPagesUI : MonoBehaviour
{
    [Header("Textos TextMeshPro")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI pageCounterText;

    [Header("Botones")]
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject finishButton;

    [Header("Paginas del tutorial")]
    [SerializeField] private TutorialPage[] pages;

    private int currentPageIndex;
    private SimplePatrolAI aiOwner;

    public void Open(SimplePatrolAI ai)
    {
        aiOwner = ai;
        currentPageIndex = 0;

        gameObject.SetActive(true);
        ShowCurrentPage();
    }

    public void NextPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            ShowCurrentPage();
        }
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowCurrentPage();
        }
    }

    public void FinishTutorial()
    {
        gameObject.SetActive(false);

        if (aiOwner != null)
            aiOwner.FinishTutorial();
    }

    private void ShowCurrentPage()
    {
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("No hay paginas cargadas en TutorialPagesUI.");
            return;
        }

        TutorialPage page = pages[currentPageIndex];

        if (titleText != null)
            titleText.text = page.title;

        if (bodyText != null)
            bodyText.text = page.body;

        if (pageCounterText != null)
            pageCounterText.text = $"{currentPageIndex + 1} / {pages.Length}";

        bool isFirstPage = currentPageIndex == 0;
        bool isLastPage = currentPageIndex == pages.Length - 1;

        if (previousButton != null)
            previousButton.SetActive(!isFirstPage);

        if (nextButton != null)
            nextButton.SetActive(!isLastPage);

        if (finishButton != null)
            finishButton.SetActive(isLastPage);
    }
}

[System.Serializable]
public class TutorialPage
{
    public string title;

    [TextArea(3, 8)]
    public string body;
}