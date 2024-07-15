using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; }

        [SerializeField] GameObject gameOverScreen;
        [SerializeField] GameObject screenUIOverlay;
        [SerializeField] TextMeshProUGUI gameOverScoreText;
        [SerializeField] RectTransform targetReticleRectTransform;
        [SerializeField] Image targetReticleImage;
        [SerializeField] RectTransform lockOnRingReticleRectTransform;
        [SerializeField] Image lockOnRingReticleImage;
        [SerializeField] Slider playerHealthSlider;
        [SerializeField] TextMeshProUGUI gameScoreText;
        [SerializeField] GameObject menuObject;
        [SerializeField] Camera mainCamera;

        [SerializeField] Queue<GameObject> lockOnReticlePool;

        protected void Awake()
        {
            if (instance == null)
            {
                //DontDestroyOnLoad(gameObject);
                instance = gameObject.GetComponent<UIManager>();
            }
            else
                Destroy(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateTargetReticlePosition(Vector3 position)
        {
            targetReticleRectTransform.position = mainCamera.WorldToScreenPoint(position);
        }

        public void UpdateTargetReticleActiveColour(bool isActive)
        {
            if (isActive)
                targetReticleImage.color = 2f * Color.red;
            else
                targetReticleImage.color = 2f * Color.white;
        }

        public void UpdateLockOnReticleRingPosition(Vector3 position)
        {
            lockOnRingReticleRectTransform.position = mainCamera.WorldToScreenPoint(position);
        }

        public void UpdateLockOnReticleRingColour(bool isActive)
        {
            if (isActive)
                lockOnRingReticleImage.color = 2f * Color.red;
            else
                lockOnRingReticleImage.color = 2f * Color.white;
        }

        public void UpdatePlayerHealthSlider(float value)
        {
            playerHealthSlider.value = value;
        }

        public void UpdateScore(int score)
        {
            gameScoreText.text = score.ToString();
        }

        public void GameOverScreen(int score)
        {
            screenUIOverlay.SetActive(false);
            gameOverScoreText.text = score.ToString();
            gameOverScreen.SetActive(true);
        }

        public void ToggleMenu()
        {
            menuObject.SetActive(true);
        }
    }
}
