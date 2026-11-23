using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    private bool isTransitioning;
    private Coroutine fadeCoroutine;

    private void Awake()
    {

        if ( Instance !=null && Instance !=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (canvasGroup != null )
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (!isTransitioning)
        {
            StartFadeIn();
        }
    }

    public void FadeToScene(string sceneName)
    {
        if(isTransitioning) return;
        if (canvasGroup == null )
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            return;
        }
        if (fadeCoroutine != null )
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts =true;
        yield return Ease.Tween(fadeDuration, t =>
        {
            canvasGroup.alpha = t;
        }, Ease.OutCubic, unscaled: true);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName );
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isTransitioning)
        {
            StartFadeIn();
        }
    }

    private void StartFadeIn()
    {
        if (canvasGroup == null)
        {
            isTransitioning = false;
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        yield return Ease.Tween(fadeDuration, t =>
        {
            canvasGroup.alpha = 1f - t;
        }, Ease.OutCubic, unscaled:true );
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts =false;
        isTransitioning = false ;
        fadeCoroutine = null;
      
    }
}