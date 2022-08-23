using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Events;

// [RequireComponent(typeof(Image))]
// [RequireComponent(typeof(Mask))]
[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{

    [Tooltip("Set starting page index - starting from 0")]
    public int startingPage = 0;
    [Tooltip("Threshold time for fast swipe in seconds")]
    public float fastSwipeThresholdTime = 0.3f;
    [Tooltip("Threshold time for fast swipe in (unscaled) pixels")]
    public int fastSwipeThresholdDistance = 100;
    [Tooltip("How fast will page lerp to target position")]
    public float decelerationRate = 10f;
    [Tooltip("Button to go to the previous page (optional)")]
    public GameObject prevButton;
    [Tooltip("Button to go to the next page (optional)")]
    public GameObject nextButton;
    [Tooltip("Sprite for unselected page (optional)")]
    public Sprite unselectedPage;
    [Tooltip("Sprite for selected page (optional)")]
    public Sprite selectedPage;
    [Tooltip("Container with page images (optional)")]
    public Transform pageSelectionIcons;

    public UnityEvent pageChangedEvent;

    // fast swipes should be fast and short. If too long, then it is not fast swipe
    private int m_fastSwipeThresholdMaxLimit;

    private ScrollRect m_scrollRectComponent;
    private RectTransform m_scrollRectRect;
    private RectTransform m_container;

    private bool m_horizontal;

    // number of pages in container
    private int m_pageCount;
    private int m_currentPage;

    // whether lerping is in progress and target lerp position
    private bool m_lerp;
    private Vector2 m_lerpTo;

    // target position of every page
    private List<Vector2> m_pagePositions = new List<Vector2>();

    // in draggging, when dragging started and where it started
    private bool m_dragging;
    private float m_timeStamp;
    private Vector2 m_startPosition;

    // for showing small page icons
    private bool m_showPageSelection;
    private int m_previousPageSelectionIndex;
    // container with Image components - one Image for each page
    private List<Image> m_pageSelectionImages;

    public int CurrentPage { get { return m_currentPage; } }
    //------------------------------------------------------------------------
    void Start()
    {
        m_scrollRectComponent = GetComponent<ScrollRect>();
        m_scrollRectRect = GetComponent<RectTransform>();
        m_container = m_scrollRectComponent.content;
        m_pageCount = m_container.childCount;

        // is it horizontal or vertical scrollrect
        if (m_scrollRectComponent.horizontal && !m_scrollRectComponent.vertical)
        {
            m_horizontal = true;
        }
        else if (!m_scrollRectComponent.horizontal && m_scrollRectComponent.vertical)
        {
            m_horizontal = false;
        }
        else
        {
            Debug.LogWarning("Confusing setting of horizontal/vertical direction. Default set to horizontal.");
            m_horizontal = true;
        }

        m_lerp = false;

        // init
        SetPagePositions();
        SetPage(startingPage);
        InitPageSelection();
        SetPageSelection(startingPage);

        // prev and next buttons
        if (nextButton)
            nextButton.GetComponent<Button>().onClick.AddListener(() => { NextScreen(); });

        if (prevButton)
            prevButton.GetComponent<Button>().onClick.AddListener(() => { PreviousScreen(); });
    }

    //------------------------------------------------------------------------
    void Update()
    {
        // if moving to target position
        if (m_lerp)
        {
            // prevent overshooting with values greater than 1
            float decelerate = Mathf.Min(decelerationRate * Time.deltaTime, 1f);
            m_container.anchoredPosition = Vector2.Lerp(m_container.anchoredPosition, m_lerpTo, decelerate);
            // time to stop lerping?
            if (Vector2.SqrMagnitude(m_container.anchoredPosition - m_lerpTo) < 0.25f)
            {
                // snap to target and stop lerping
                m_container.anchoredPosition = m_lerpTo;
                m_lerp = false;
                // clear also any scrollrect move that may interfere with our lerping
                m_scrollRectComponent.velocity = Vector2.zero;
            }

            // switches selection icon exactly to correct page
            if (m_showPageSelection)
            {
                SetPageSelection(GetNearestPage());
            }
        }
    }

    //------------------------------------------------------------------------
    private void SetPagePositions()
    {
        int width = 0;
        int height = 0;
        int offsetX = 0;
        int offsetY = 0;
        int containerWidth = 0;
        int containerHeight = 0;

        if (m_horizontal)
        {
            // screen width in pixels of scrollrect window
            width = (int)m_scrollRectRect.rect.width;
            // center position of all pages
            offsetX = width / 2;
            // total width
            containerWidth = width * m_pageCount;
            // limit fast swipe length - beyond this length it is fast swipe no more
            m_fastSwipeThresholdMaxLimit = width;
        }
        else
        {
            height = (int)m_scrollRectRect.rect.height;
            offsetY = height / 2;
            containerHeight = height * m_pageCount;
            m_fastSwipeThresholdMaxLimit = height;
        }

        // set width of container
        Vector2 newSize = new Vector2(containerWidth, containerHeight);
        m_container.sizeDelta = newSize;
        Vector2 newPosition = new Vector2(containerWidth / 2, containerHeight / 2);
        m_container.anchoredPosition = newPosition;

        // delete any previous settings
        m_pagePositions.Clear();

        // iterate through all container childern and set their positions
        for (int i = 0; i < m_pageCount; i++)
        {
            RectTransform child = m_container.GetChild(i).GetComponent<RectTransform>();
            Vector2 childPosition;
            if (m_horizontal)
            {
                childPosition = new Vector2(i * width - containerWidth / 2 + offsetX, 0f);
            }
            else
            {
                childPosition = new Vector2(0f, -(i * height - containerHeight / 2 + offsetY));
            }
            child.anchoredPosition = childPosition;
            m_pagePositions.Add(-childPosition);
        }
    }

    //------------------------------------------------------------------------
    private void SetPage(int aPageIndex)
    {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, m_pageCount - 1);
        m_container.anchoredPosition = m_pagePositions[aPageIndex];
        m_currentPage = aPageIndex;
        if (pageChangedEvent != null)
        {
            pageChangedEvent.Invoke();
        }
    }

    //------------------------------------------------------------------------
    private void LerpToPage(int aPageIndex)
    {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, m_pageCount - 1);
        m_lerpTo = m_pagePositions[aPageIndex];
        m_lerp = true;
        m_currentPage = aPageIndex;

        if (pageChangedEvent != null)
        {
            pageChangedEvent.Invoke();
        }
    }

    //------------------------------------------------------------------------
    private void InitPageSelection()
    {
        // page selection - only if defined sprites for selection icons
        m_showPageSelection = unselectedPage != null && selectedPage != null;
        if (m_showPageSelection)
        {
            // also container with selection images must be defined and must have exatly the same amount of items as pages container
            if (pageSelectionIcons == null || pageSelectionIcons.childCount != m_pageCount)
            {
                Debug.LogWarning("Different count of pages and selection icons - will not show page selection");
                m_showPageSelection = false;
            }
            else
            {
                m_previousPageSelectionIndex = -1;
                m_pageSelectionImages = new List<Image>();

                // cache all Image components into list
                for (int i = 0; i < pageSelectionIcons.childCount; i++)
                {
                    Image image = pageSelectionIcons.GetChild(i).GetComponent<Image>();
                    if (image == null)
                    {
                        Debug.LogWarning("Page selection icon at position " + i + " is missing Image component");
                    }
                    m_pageSelectionImages.Add(image);
                }
            }
        }
    }

    //------------------------------------------------------------------------
    private void SetPageSelection(int aPageIndex)
    {
        // nothing to change
        if (m_previousPageSelectionIndex == aPageIndex)
        {
            return;
        }

        // unselect old
        if (m_previousPageSelectionIndex >= 0)
        {
            m_pageSelectionImages[m_previousPageSelectionIndex].sprite = unselectedPage;
            m_pageSelectionImages[m_previousPageSelectionIndex].SetNativeSize();
        }

        // select new
        m_pageSelectionImages[aPageIndex].sprite = selectedPage;
        m_pageSelectionImages[aPageIndex].SetNativeSize();

        m_previousPageSelectionIndex = aPageIndex;
    }

    //------------------------------------------------------------------------
    private void NextScreen()
    {
        LerpToPage(m_currentPage + 1);
    }

    //------------------------------------------------------------------------
    private void PreviousScreen()
    {
        LerpToPage(m_currentPage - 1);
    }

    //------------------------------------------------------------------------
    private int GetNearestPage()
    {
        // based on distance from current position, find nearest page
        Vector2 currentPosition = m_container.anchoredPosition;

        float distance = float.MaxValue;
        int nearestPage = m_currentPage;

        for (int i = 0; i < m_pagePositions.Count; i++)
        {
            float testDist = Vector2.SqrMagnitude(currentPosition - m_pagePositions[i]);
            if (testDist < distance)
            {
                distance = testDist;
                nearestPage = i;
            }
        }

        return nearestPage;
    }

    //------------------------------------------------------------------------
    public void OnBeginDrag(PointerEventData aEventData)
    {
        // if currently lerping, then stop it as user is draging
        m_lerp = false;
        // not dragging yet
        m_dragging = false;
    }

    //------------------------------------------------------------------------
    public void OnEndDrag(PointerEventData aEventData)
    {
        // how much was container's content dragged
        float difference;
        if (m_horizontal)
        {
            difference = m_startPosition.x - m_container.anchoredPosition.x;
        }
        else
        {
            difference = -(m_startPosition.y - m_container.anchoredPosition.y);
        }

        // test for fast swipe - swipe that moves only +/-1 item
        if (Time.unscaledTime - m_timeStamp < fastSwipeThresholdTime &&
            Mathf.Abs(difference) > fastSwipeThresholdDistance &&
            Mathf.Abs(difference) < m_fastSwipeThresholdMaxLimit)
        {
            if (difference > 0)
            {
                NextScreen();
            }
            else
            {
                PreviousScreen();
            }
        }
        else
        {
            // if not fast time, look to which page we got to
            LerpToPage(GetNearestPage());
        }

        m_dragging = false;
    }

    //------------------------------------------------------------------------
    public void OnDrag(PointerEventData aEventData)
    {
        if (!m_dragging)
        {
            // dragging started
            m_dragging = true;
            // save time - unscaled so pausing with Time.scale should not affect it
            m_timeStamp = Time.unscaledTime;
            // save current position of cointainer
            m_startPosition = m_container.anchoredPosition;
        }
        else
        {
            if (m_showPageSelection)
            {
                SetPageSelection(GetNearestPage());
            }
        }
    }
}
