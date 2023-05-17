using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.common.TimelineReveal.Editor
{
    public abstract class UnresolvedReferenceClipEditorBase : ClipEditor
    {
        protected abstract bool IsFullyResolved(TimelineClip timelineClip, PlayableDirector director);

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            if (TimelineEditor.masterAsset && TimelineEditor.masterDirector)
            {
                var isPartOfPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(TimelineEditor.masterDirector.gameObject);
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                var isPrefabScene =
                    prefabStage && (TimelineEditor.masterDirector.gameObject.scene == prefabStage.scene);

                if (!isPartOfPrefabAsset && !isPrefabScene)
                {
                    var resolved = IsFullyResolved(clip, TimelineEditor.masterDirector);
                    if (!resolved)
                    {
                        DrawOutline(region.position, 2f, Color.magenta);
                        EditorGUI.LabelField(Inflated(region.position, Vector2.one * -2),
                            EditorGUIUtility.IconContent("console.erroricon"));
                        return;
                    }
                }
            }

            base.DrawBackground(clip, region);
        }

        private static Rect Inflated(Rect r, Vector2 delta)
        {
            return new Rect(
                r.xMin - delta.x,
                r.yMin - delta.y,
                r.width + delta.x * 2,
                r.height + delta.y * 2);
        }

        private static void DrawOutline(Rect rect, float size, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var resetColor = GUI.color;
            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1f, size, rect.height - 2f * size),
                EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - size, rect.y + 1f, size, rect.height - 2f * size),
                EditorGUIUtility.whiteTexture);
            GUI.color = resetColor;
        }
    }
}