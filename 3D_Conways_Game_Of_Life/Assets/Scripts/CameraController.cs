using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{

    /// <summary>
    /// Handles the movement of camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {

        public class SetupCameraEvent : UnityEvent<int, int, int, float>
        {
        }

        public static SetupCameraEvent SetupCamera;
        private Coroutine resetCameraCoroutine;
        private bool isCameraResetting;
        private Camera _camera;
        private Vector3 originalPostion;
        private MinMax<float> orthographicSize;
        private MinMax<float> resetCameraSpeed;
        private Bounds bounds;
        private Vector3 initialMousePosition;
        private Vector3 targetRotation;

        private const float RotationSpeed = 50.0f;
        private const float MoveSpeed = 50.0f;
        private const float PanSpeed = 2.5f;
        private const float ZoomSpeed = 100f;
        private float zoomInputDelta;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (SetupCamera == null)
            {
                SetupCamera = new SetupCameraEvent();
            }

            resetCameraSpeed.Min = 0.01f;
            resetCameraSpeed.Max = 5.0f;
        }

        private void OnEnable()
        {
            SetupCamera.AddListener(Setup);
        }

        private void OnDisable()
        {
            SetupCamera.RemoveListener(Setup);
        }

        private void Update()
        {
            if (Manager.GameState != GameStateEnum.AcceptInput && Manager.GameState != GameStateEnum.Run) return;
            if (!isCameraResetting && Input.GetKeyDown(KeyCode.R))
            {
                isCameraResetting = true;
                resetCameraCoroutine = StartCoroutine(ResetCamera());
            }

            if (_camera.orthographic)
            {
                DetermineZoomDelta();
            }
            else
            {
                if (!Input.GetMouseButton(0)) return;
                if (isCameraResetting && resetCameraCoroutine != null)
                {
                    StopCoroutine(resetCameraCoroutine);
                    isCameraResetting = false;
                }
                CalculateNewRotaion();
            }
        }

        private void LateUpdate()
        {
            if (Manager.GameState != GameStateEnum.AcceptInput && Manager.GameState != GameStateEnum.Run) return;
            if (_camera.orthographic)
            {
                ZoomCamera();
                PanCamera();
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    transform.eulerAngles =
                        Vector3.Lerp(transform.eulerAngles, targetRotation, RotationSpeed * Time.smoothDeltaTime);
                }

                MoveCamera();
            }
        }

        /// <summary>
        /// Setups the camera values for 3d and 2d.
        /// </summary>
        /// <param name="gridWidth">Width of grid.</param>
        /// <param name="gridHeight">Height of grid.</param>
        /// <param name="gridDepth">Depth of grid.</param>
        /// <param name="distanceMultiplierFor3D">Distance between cells in 3d</param>
        private void Setup(int gridWidth, int gridHeight, int gridDepth, float distanceMultiplierFor3D)
        {
            var isWidthEven = (gridWidth & 0x1) == 0;
            var isHeightEven = (gridHeight & 0x1) == 0;
            if (gridDepth == 1)
            {
                _camera.orthographic = true;

                originalPostion = transform.position =
                    new Vector3(isWidthEven ? -0.5f : 0.0f, isHeightEven ? -0.5f : 0.0f, -10.0f);

                var aspectRatioMultiplier = _camera.aspect >= 1.0f ? 1.0f : 1.0f / _camera.aspect;
                orthographicSize.Max = _camera.orthographicSize =
                    aspectRatioMultiplier * 0.5f * Mathf.Max(gridWidth, gridHeight);
                orthographicSize.Min = aspectRatioMultiplier - 0.5f;
                bounds = new Bounds(_camera, orthographicSize.Max);
                bounds.Update(_camera);
            }
            else
            {
                _camera.orthographic = false;

                originalPostion = transform.position =
                    new Vector3(isWidthEven ? -0.5f : 0.0f, isHeightEven ? -0.5f : 0.0f,
                        -distanceMultiplierFor3D * Mathf.Max(gridWidth, gridHeight));
            }
        }

        private void CalculateNewRotaion()
        {
            targetRotation = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0.0f);

            targetRotation += transform.eulerAngles;
        }

        private void MoveCamera()
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                if (isCameraResetting && resetCameraCoroutine != null)
                {
                    StopCoroutine(resetCameraCoroutine);
                    isCameraResetting = false;
                }
            }
            var time = Time.smoothDeltaTime * MoveSpeed;
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * time;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * time;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * time;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * time;
            }
        }

        private IEnumerator ResetCamera()
        {
            var time = 0.0f;
            yield return new WaitUntil(() =>
            {
                time += Time.smoothDeltaTime * Mathf.Lerp(resetCameraSpeed.Min, resetCameraSpeed.Max, Time.smoothDeltaTime);
                transform.SetPositionAndRotation(Vector3.Lerp(transform.position, originalPostion, time), Quaternion.Lerp(transform.rotation, Quaternion.identity, time));

                if (_camera.orthographic)
                {
                    _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, orthographicSize.Max, time);
                }
                return time > 1.0f;
            });
            if (_camera.orthographic)
            {
                UpdateBoundsAfterReset();
            }
        }

        private void UpdateBoundsAfterReset()
        {
            bounds.Update(_camera);
            transform.position = bounds.ClampCamera(transform.position);
            isCameraResetting = false;
        }

        private void DetermineZoomDelta()
        {
            zoomInputDelta = -Input.GetAxis("Mouse ScrollWheel");
        }

        private void ZoomCamera()
        {
            if (!(Mathf.Abs(zoomInputDelta) > 0.0f)) return;
            if (isCameraResetting && resetCameraCoroutine != null)
            {
                StopCoroutine(resetCameraCoroutine);
                UpdateBoundsAfterReset();
            }
            _camera.orthographicSize += zoomInputDelta * Time.smoothDeltaTime * ZoomSpeed;
            _camera.orthographicSize =
                Mathf.Clamp(_camera.orthographicSize, orthographicSize.Min, orthographicSize.Max);
            bounds.Update(_camera);
            zoomInputDelta = 0.0f;
            transform.position = bounds.ClampCamera(transform.position);
        }

        private void PanCamera()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (isCameraResetting && resetCameraCoroutine != null)
                {
                    StopCoroutine(resetCameraCoroutine);
                    UpdateBoundsAfterReset();
                }
                initialMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                var direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) -
                                Camera.main.ScreenToWorldPoint(initialMousePosition);
                direction.z = 0.0f;

                var position = transform.position + direction;

                var newPosition = Vector3.Lerp(transform.position,
                    new Vector3(position.x, position.y, transform.position.z), Time.smoothDeltaTime * PanSpeed);
                transform.position = bounds.ClampCamera(newPosition);
            }
        }
    }

    public class Bounds
    {
        private readonly float hExtent;
        private readonly float vExtent;
        private readonly float maxOrthographicSize;

        private Vector3 bottomLeft;
        private Vector3 topRight;

        private float left;
        private float right;
        private float top;
        private float bottom;

        public Bounds(Camera camera, float max)
        {
            vExtent = camera.orthographicSize;
            hExtent = camera.aspect * vExtent;

            bottomLeft = camera.ViewportToWorldPoint(Vector2.zero);
            topRight = camera.ViewportToWorldPoint(Vector2.one);

            maxOrthographicSize = max;
        }

        public void Update(Camera camera)
        {
            var deltaOrthographicSize = maxOrthographicSize - camera.orthographicSize;

            var vDeltaExtent = deltaOrthographicSize;
            var hDeltaExtent = camera.aspect * vDeltaExtent;

            left = bottomLeft.x + hExtent - hDeltaExtent;
            right = topRight.x - hExtent + hDeltaExtent;
            bottom = bottomLeft.y + vExtent - vDeltaExtent;
            top = topRight.y - vExtent + vDeltaExtent;
        }

        public Vector3 ClampCamera(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, left, right);
            position.y = Mathf.Clamp(position.y, bottom, top);
            return position;
        }
    }

    public struct MinMax<T>
    {
        public T Min;
        public T Max;
    }
}
