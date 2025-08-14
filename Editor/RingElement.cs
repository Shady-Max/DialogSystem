using UnityEngine;
using UnityEngine.UIElements;

namespace ShadyMax.DialogSystem.Editor
{
    public class RingElement : VisualElement
    {
        public Color[] fillColors = {Color.gray};
        public float thickness = 1f;
        
        public RingElement()
        {
            style.position = Position.Absolute;
            style.top = 0;
            style.left = 0;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);
            generateVisualContent += OnGenerateVisualContent;
        }
 
        public void SetColors(params Color[] colors)
        {
            fillColors = colors;
            MarkDirtyRepaint();
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            float outerRadius = Mathf.Min(contentRect.width, contentRect.height) / 2f + 1f;
            float centerRadius = outerRadius - thickness / 2;

            Vector2 center = new Vector2(contentRect.width / 2f, contentRect.height / 2f);

            painter.lineWidth = thickness;
            painter.fillColor = Color.clear;

            float angleStep = 360f / fillColors.Length; // radians per segment
            float gap = 0.5f; // small gap to prevent overlap

            float currentAngle = 0f;

            for (int i = 0; i < fillColors.Length; i++)
            {
                var color = fillColors[i];
                color.a = 1f;
                painter.strokeColor = color;

                float startAngle = currentAngle;
                float endAngle = currentAngle + angleStep - gap;

                painter.BeginPath();
                painter.Arc(center, centerRadius, startAngle, endAngle, ArcDirection.Clockwise);
                painter.Stroke();

                currentAngle += angleStep;
            }
        }
    }
}