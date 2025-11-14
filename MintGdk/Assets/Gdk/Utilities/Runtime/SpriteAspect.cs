using System.Collections;
using Unity.Collections;
using UnityEngine;
namespace Mint.Gdk.Utilities.Runtime
{
    public enum SpriteAspectType
    {
        WidthControlHeight,
        HeightControlWidth,
        FitParent,
        EnvelopParent,
    }

    public class SpriteAspect : MonoBehaviour
    {
        [SerializeField] private SpriteAspectType _aspectType;
        [SerializeField, ReadOnly] private float _selfRatio;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [SerializeField] SpriteRenderer _parentSr;
        [SerializeField] bool _autoUpdate = true;
        private SpriteRenderer _mySr;
        void Awake()
        {
            if (_mySr == null)
            {
                _mySr = this.GetComponent<SpriteRenderer>();
            }

            if (_autoUpdate)
            {
                CoroutineHandler.GetStaticCoroutineRunner().StartCoroutine(UpdateTransformCoroutine());
            }
        }
        void OnEnable()
        {
            if (_mySr == null)
            {
                _mySr = this.GetComponent<SpriteRenderer>();
            }

            if (_autoUpdate)
            {
                CoroutineHandler.GetStaticCoroutineRunner().StartCoroutine(UpdateTransformCoroutine());
            }
        }

        void OnValidate()
        {
            if (_mySr == null)
                _mySr = this.GetComponent<SpriteRenderer>();
            UpdateTransform();
        }

        IEnumerator UpdateTransformCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            UpdateTransform();
        }

        [ContextMenu("UpdateTransform")]
        public void UpdateTransform()
        {
            switch (_aspectType)
            {
                case SpriteAspectType.WidthControlHeight:
                    _selfRatio = this.transform.lossyScale.x / this.transform.lossyScale.y;
                    float newY = HeightBaseOnWidth(this.transform.localScale.x);
                    this.transform.localScale = new Vector2(this.transform.localScale.x, newY);
                    Debug.Log("LocalScale: " + this.transform.localScale);
                    Debug.Log("LossyScale: " + this.transform.lossyScale);
                    break;
                case SpriteAspectType.HeightControlWidth:
                    _selfRatio = this.transform.lossyScale.y / this.transform.lossyScale.x;
                    float newX = WidthBaseOnHeight(this.transform.localScale.y);
                    this.transform.localScale = new Vector2(newX, this.transform.localScale.y);
                    break;
                case SpriteAspectType.FitParent:
                    this.transform.localScale = CalculateNewXY(true);
                    break;
                case SpriteAspectType.EnvelopParent:
                    this.transform.localScale = CalculateNewXY(false);
                    break;
            }
            _selfRatio = this.transform.lossyScale.x / this.transform.lossyScale.y;
        }

        public void UpdateTransform(Vector2 parentBounds)
        {
            switch (_aspectType)
            {
                case SpriteAspectType.WidthControlHeight:
                    _selfRatio = this.transform.lossyScale.x / this.transform.lossyScale.y;
                    float newY = HeightBaseOnWidth(this.transform.localScale.x);
                    this.transform.localScale = new Vector2(this.transform.localScale.x, newY);
                    break;
                case SpriteAspectType.HeightControlWidth:
                    _selfRatio = this.transform.lossyScale.y / this.transform.lossyScale.x;
                    float newX = WidthBaseOnHeight(this.transform.localScale.y);
                    this.transform.localScale = new Vector2(newX, this.transform.localScale.y);
                    break;
                case SpriteAspectType.FitParent:
                    this.transform.localScale = CalculateNewXY(parentBounds, true);
                    break;
                case SpriteAspectType.EnvelopParent:
                    this.transform.localScale = CalculateNewXY(parentBounds, false);
                    break;
            }
            _selfRatio = this.transform.lossyScale.x / this.transform.lossyScale.y;
        }

        public void SetSpriteParent(SpriteRenderer parentSr)
        {
            _parentSr = parentSr;
        }

        public void SetAspectType(SpriteAspectType aspectType)
        {
            _aspectType = aspectType;
        }

        private float HeightBaseOnWidth(float width)
        {
            Vector2 oldScale = this.transform.localScale;
            float widthRatio = width / oldScale.x;
            _selfRatio = this.transform.lossyScale.x / this.transform.lossyScale.y;
            float newY = oldScale.y * widthRatio * _selfRatio;
            return newY;
        }

        private float WidthBaseOnHeight(float height)
        {
            Vector2 oldScale = this.transform.localScale;
            float heightRatio = height / oldScale.y;
            _selfRatio = this.transform.lossyScale.y / this.transform.lossyScale.x;
            float newX = oldScale.x * heightRatio * _selfRatio;
            return newX;
        }
        private Vector2 CalculateNewXY(Vector2 parentBounds, bool isMax)
        {
            Vector2 meBounds = _mySr.bounds.size;
            float childVsParentRatio = meBounds.x / parentBounds.x;
            Vector2 oldScale = this.transform.localScale;
            float newX, newY;
            if (isMax)
            {
                if (meBounds.x > meBounds.y)
                {
                    childVsParentRatio = parentBounds.x / meBounds.x;
                    newX = oldScale.x * childVsParentRatio;
                    newY = HeightBaseOnWidth(newX);
                }
                else
                {
                    childVsParentRatio = parentBounds.y / meBounds.y;
                    newY = oldScale.y * childVsParentRatio;
                    newX = WidthBaseOnHeight(newY);
                }
            }
            else
            {
                if (meBounds.x < meBounds.y)
                {
                    childVsParentRatio = parentBounds.x / meBounds.x;
                    newX = oldScale.x * childVsParentRatio;
                    newY = HeightBaseOnWidth(newX);
                }
                else
                {
                    childVsParentRatio = parentBounds.y / meBounds.y;
                    newY = oldScale.y * childVsParentRatio;
                    newX = WidthBaseOnHeight(newY);
                }
            }
            newX -= _offset.x;
            newY -= _offset.y;

            return new Vector2(newX, newY);
        }
        private Vector2 CalculateNewXY(bool isMax)
        {
            if (_parentSr == null)
            {
                _parentSr = this.transform.parent.TryGetComponent<SpriteRenderer>(out var sr) ? sr : null;
            }
            if (_mySr == null || _parentSr == null || _mySr.sprite == null || _parentSr.sprite == null) return Vector2.one;
            Vector2 parentBounds = _parentSr.bounds.size;
            return CalculateNewXY(parentBounds, isMax);
        }
    }
}
