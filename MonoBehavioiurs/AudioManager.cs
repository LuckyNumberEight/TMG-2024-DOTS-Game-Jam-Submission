using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LuckyNumber8.TMG2024DOTSJAM
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] AudioSource ciwsFiringFXAudioSource;
        [SerializeField] AudioSource cannonShootFXAudioSource;
        [SerializeField] AudioSource missileLaunchFXAudioSource;
        [SerializeField] AudioSource explosionFXAudioSource;
        [SerializeField] AudioSource bgmAudsioSource;


        [Header("Audio Clips")]
        [SerializeField] AudioClip bgmStartAudioClip;
        [SerializeField] AudioClip bgmLoopAudioClip;
        [SerializeField] AudioClip gameOverAudioClip;
        [SerializeField] AudioClip ciwsFiringAudioClip;
        [SerializeField] AudioClip cannonShootingAudioClip;
        [SerializeField] AudioClip missileLaunchAudioClip;
        [SerializeField] AudioClip explosionAudioClip;
        [SerializeField] float bgmTimer = 68.0f;

        protected void Awake()
        {
            if (instance == null)
            {
                //DontDestroyOnLoad(gameObject);
                instance = gameObject.GetComponent<AudioManager>();
            }
            else
                Destroy(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            ciwsFiringFXAudioSource.clip = ciwsFiringAudioClip;
            cannonShootFXAudioSource.clip = cannonShootingAudioClip;
            missileLaunchFXAudioSource.clip = missileLaunchAudioClip;
            explosionFXAudioSource.clip = explosionAudioClip;
        }

        // Update is called once per frame
        void Update()
        {
            if (bgmTimer > 0f)
            {
                bgmTimer -= Time.deltaTime;

                if (bgmTimer > 0f)
                    return;

                bgmAudsioSource.clip = bgmLoopAudioClip;
                bgmAudsioSource.Play();
            }
        }

        public void PlayCIWSFiringFX()
        {
            ciwsFiringFXAudioSource.pitch = Random.Range(0.9f, 1.1f);
            ciwsFiringFXAudioSource.Play();
        }


        public void PlayCannonShootFX()
        {
            cannonShootFXAudioSource.pitch = Random.Range(0.9f, 1.1f);
            cannonShootFXAudioSource.Play();
        }

        public void PlayMissileLaunchFX()
        {
            missileLaunchFXAudioSource.pitch = Random.Range(0.9f, 1.1f);
            missileLaunchFXAudioSource.Play();
        }

        public void PlayExplosionFX()
        {
            explosionFXAudioSource.pitch = Random.Range(0.9f, 1.1f);
            explosionFXAudioSource.Play();
        }

        public void PlayGameOverBGM()
        {
            bgmAudsioSource.clip = gameOverAudioClip;
            bgmAudsioSource.Play();
        }
    }
}
