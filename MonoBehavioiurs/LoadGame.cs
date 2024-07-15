using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Scenes;
using Unity.Entities;
using UnityEngine.UI;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    public class LoadGame : MonoBehaviour
    {
        [SerializeField] GameObject loadingScreen;
        [SerializeField] Slider loadingSlider;
        [SerializeField] SubScene subScene;


        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        IEnumerator LoadSceneAsync(int sceneID)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

            loadingScreen.SetActive(true);

            while(!operation.isDone)
            {
                loadingSlider.value = Mathf.Clamp01(operation.progress / 0.9f);
                yield return null;
            }
        }

        public void PressStartGame ()
        {
            StartCoroutine(LoadSceneAsync(1));
        }

        public void PressRestart()
        {
            var defaultWorld = World.DefaultGameObjectInjectionWorld;

            defaultWorld.EntityManager.CompleteAllTrackedJobs();

            foreach (var system in defaultWorld.Systems)
            {
                system.Enabled = false;
            }
            defaultWorld.Dispose();
            DefaultWorldInitialization.Initialize("Default World", false);
            if (!ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld))
            {
                ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
            }
            StartCoroutine(LoadSceneAsync(1));
        }

        public void PressReturnToTitleScreen()
        {
            var defaultWorld = World.DefaultGameObjectInjectionWorld;

            defaultWorld.EntityManager.CompleteAllTrackedJobs();

            foreach (var system in defaultWorld.Systems)
            {
                system.Enabled = false;
            }
            defaultWorld.Dispose();
            DefaultWorldInitialization.Initialize("Default World", false);
            if (!ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld))
            {
                ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
            }
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            // save any game data here
            #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            # endif
        }

        public void CleanAndRestartECS()
        {

        }
    }
}
