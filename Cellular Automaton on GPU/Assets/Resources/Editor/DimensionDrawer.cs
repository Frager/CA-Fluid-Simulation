using UnityEngine;


namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(DimensionAttribute))]
    public class DimensionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DimensionAttribute rangeAttribute = (DimensionAttribute)base.attribute;

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                label.text = property.name + ": " + property.intValue * 16;
                EditorGUI.IntSlider(position, property, 1, 20, label);
            }
        }
    }
}