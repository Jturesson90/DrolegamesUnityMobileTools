namespace Drolegames.SocialService
{
    using UnityEngine;
    using UnityEngine.UI;

    public class AchievementButton : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        [Header("Settings")]
        public Sprite googlePlaySprite;
        public Sprite iTunesSprite;
        public Sprite mockSprite;

        private void Awake()
        {
            if (!image || !button)
            {
                Debug.LogError(gameObject.name + " is missing scene references");
            }
        }
        private void Start()
        {
            var isAlreadyLoggedIn = SocialManager.IsInitialized && SocialManager.Current.IsLoggedIn;
            button.gameObject.SetActive(isAlreadyLoggedIn);
            if (isAlreadyLoggedIn)
            {
                UpdateImage(SocialManager.Current.Platform);
            }
            SocialManager.Current.LoggedInChanged += SocialManager_LoggedInChanged;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
        private void OnDestroy()
        {
            if (SocialManager.IsInitialized)
            {
                SocialManager.Current.LoggedInChanged -= SocialManager_LoggedInChanged;
            }
        }

        private void OnButtonClicked()
        {
            if (SocialManager.IsInitialized)
            {
                SocialManager.Current.Achievements.ShowUI();
            }
        }

        private void SocialManager_LoggedInChanged(object sender, SocialManagerArgs e)
        {
            button.gameObject.SetActive(e.IsLoggedIn);
            UpdateImage(e.Platform);
        }

        private void UpdateImage(RuntimePlatform platform)
        {

            if (platform == RuntimePlatform.Android)
            {
                image.sprite = googlePlaySprite;
            }
            else if (platform == RuntimePlatform.Android)
            {
                image.sprite = iTunesSprite;
            }
            else
            {
                image.sprite = mockSprite;
            }
        }
    }
}