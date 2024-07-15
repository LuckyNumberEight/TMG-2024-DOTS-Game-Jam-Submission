using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    public class Gizmos : MonoBehaviour
    {
        PlayerInputActions playerInputActions;
        Vector3 position;
        Vector3 direction;
        // Start is called before the first frame update
        void Start()
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
        }

        // Update is called once per frame
        void Update()
        {

            var mouseInput = playerInputActions.KeyboardMap.MousePosition.ReadValue<Vector2>();
            var ray = Camera.main.ScreenPointToRay(mouseInput);
            var plane = new Plane(Vector3.up, 0);
            float distance;
            bool isHit = plane.Raycast(ray, out distance);
            position = ray.GetPoint(distance);
            direction = Camera.main.transform.position - position;
            
        }

        private void OnDrawGizmos()
        {
            UnityEngine.Gizmos.color = Color.red;
            UnityEngine.Gizmos.DrawLine(Camera.main.transform.position, position);
            UnityEngine.Gizmos.DrawWireSphere(position, 4f);
        }
    }
}
