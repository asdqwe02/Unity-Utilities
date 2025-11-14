using UnityEngine;

namespace Mint.Gdk.Utilities.Runtime
{
    [RequireComponent(typeof(Transform))]
    public class TransformAnchor2D : MonoBehaviour
    {
        [SerializeField] Vector2 _anchor = Vector2.zero;
        [SerializeField] Vector2 _offset = Vector2.zero;
        private Camera _camera;
        private Transform _transform; // cache transform

        public Vector2 Anchor => _anchor;
        public Vector2 Offset => _offset;
        void Awake()
        {
            _transform = transform;
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        public void Setup(Camera camera)
        {
            _camera = camera;
            UpdatePosition();
        }

        void OnEnable()
        {
            UpdatePosition();
        }

        public void SetAnchor(Vector2 anchor)
        {
            _anchor = anchor;
        }
        public void SetOffset(Vector2 offset)
        {
            _offset = offset;
        }

        public void UpdatePosition()
        {
            if (_camera == null) return;

            Vector3 viewportPosition = new Vector3(_anchor.x, _anchor.y, _transform.position.z);
            Vector3 worldPosition = _camera.ViewportToWorldPoint(viewportPosition);
            _transform.position = worldPosition + (Vector3)_offset;
        }
    }
}
