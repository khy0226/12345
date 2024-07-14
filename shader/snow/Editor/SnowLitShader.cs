using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class SnowLitShader : BaseShaderGUI
    {
        static readonly string[] workflowModeNames = Enum.GetNames(typeof(SnowLitGUI.WorkflowMode));

        private SnowLitGUI.LitProperties litProperties;
        private SnowLitDetailGUI.LitProperties litDetailProperties;


        protected class StylesS
        {

            public static GUIContent snowDirectionalText = EditorGUIUtility.TrTextContent("Snow Directional",
                "눈 방향.");
            
            public static GUIContent additionalMapColorText = EditorGUIUtility.TrTextContent("Additional Color",
                "눈 색.");

            public static GUIContent additionalMapOnText = EditorGUIUtility.TrTextContent("Additional Map",
                "눈 텍스쳐 사용여부.");
            
            public static GUIContent additionalMapText = EditorGUIUtility.TrTextContent("Base Map",
                "눈 베이스(RGB) 눈 스무스니스(A)");
            
            public static GUIContent additionalSmoothnessText = EditorGUIUtility.TrTextContent("Smoothness",
                "스무스니스 세기");
            
            public static GUIContent additionalNormalText = EditorGUIUtility.TrTextContent("Normal Map",
                "눈 노멀 텍스쳐");
            
            public static GUIContent additionalNormalScaleText = EditorGUIUtility.TrTextContent("Additional Normal Scale",
                "눈 노멀 세기");
            
            public static GUIContent blendScaleText = EditorGUIUtility.TrTextContent("Blend Scale",
                "눈 덮힘 조절");
                        
            public static GUIContent blendNormalText = EditorGUIUtility.TrTextContent("Blend Normal",
                "눈 덮힘 모양 변경");

            public static GUIContent noiseOnText = EditorGUIUtility.TrTextContent("Noise",
                "눈 덮힘 모양 노이즈 추가여부");

            public static GUIContent noiseMapText = EditorGUIUtility.TrTextContent("Noise Map",
                "눈 덮힘 모양 노이즈 추가");
            
            public static GUIContent noiseIntensityText = EditorGUIUtility.TrTextContent("Noise Intensity",
                "눈 덮힘 모양 노이즈 조절");
            
            public static GUIContent vertexPaintText = EditorGUIUtility.TrTextContent("Vertex Paint",
                "사용하면 버텍스컬러로 빨간색이면 눈이 안덮힘");
        }



        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            materialScopesList.RegisterHeaderScope(SnowLitDetailGUI.Styles.detailInputs, Expandable.Details, _ => SnowLitDetailGUI.DoDetailArea(litDetailProperties, materialEditor));
        }

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new SnowLitGUI.LitProperties(properties);
            litDetailProperties = new SnowLitDetailGUI.LitProperties(properties);
        }

        // material changed check
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, SnowLitGUI.SetMaterialKeywords, SnowLitDetailGUI.SetMaterialKeywords);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            if (litProperties.workflowMode != null)
                DoPopup(SnowLitGUI.Styles.workflowModeText, litProperties.workflowMode, workflowModeNames);

            base.DrawSurfaceOptions(material);
        }

        public void DrawSnowInputs(Material material)       //Advanced Options 에 넣을 눈 관련 설정들
        {
            // Vector4를 가져와 XYZ를 편집
            Vector4 vector4Value = litProperties.snowDirectional.vectorValue;

            // Vector3로 변환하여 XYZ만 표시
            Vector3 vector3Value = new Vector3(vector4Value.x, vector4Value.y, vector4Value.z);

            // XYZ 값만 표시
            vector3Value = EditorGUILayout.Vector3Field(StylesS.snowDirectionalText, vector3Value);

            // XYZ 값을 업데이트하고 W 값은 유지
            litProperties.snowDirectional.vectorValue = new Vector4(vector3Value.x, vector3Value.y, vector3Value.z, vector4Value.w);

//            materialEditor.ShaderProperty(litProperties.snowDirectional, StylesS.snowDirectionalText, 1);

            EditorGUILayout.Space(10);


//            materialEditor.ShaderProperty(litProperties.additionalMapColor, StylesS.additionalMapColorText, 0);
            materialEditor.ShaderProperty(litProperties.additionalMapOn, StylesS.additionalMapOnText, 0);
            

            if (material.GetInt("_AdditionalMapOn") == 1)
            {
                materialEditor.TexturePropertySingleLine(StylesS.additionalMapText, litProperties.additionalMap, litProperties.additionalMapColor);
                material.SetTexture("_AdditionalMap", litProperties.additionalMap.textureValue);

                materialEditor.ShaderProperty(litProperties.additionalSmoothness, StylesS.additionalSmoothnessText, 2);


                if (material.GetTexture("_AdditionalNormal") != null)
                {
                    materialEditor.TexturePropertySingleLine(StylesS.additionalNormalText, litProperties.additionalNormal, litProperties.additionalNormalScale);
                }
                else
                {
                    materialEditor.TexturePropertySingleLine(StylesS.additionalNormalText, litProperties.additionalNormal);
                }
                materialEditor.TextureScaleOffsetProperty(litProperties.additionalMap);
                var additionalMapTiling = litProperties.additionalMap.textureScaleAndOffset;
                material.SetTextureScale("_AdditionalMap", new Vector2(additionalMapTiling.x, additionalMapTiling.y));
                material.SetTextureOffset("_AdditionalMap", new Vector2(additionalMapTiling.z, additionalMapTiling.w));
            }
            if (material.GetInt("_AdditionalMapOn") == 0)
            {
                materialEditor.ShaderProperty(litProperties.additionalMapColor, StylesS.additionalMapColorText, 2);
                materialEditor.ShaderProperty(litProperties.additionalSmoothness, StylesS.additionalSmoothnessText, 2);
            }

            EditorGUILayout.Space(10);

            materialEditor.ShaderProperty(litProperties.blendScale, StylesS.blendScaleText, 0);
            materialEditor.ShaderProperty(litProperties.blendNormal, StylesS.blendNormalText, 0);


            EditorGUILayout.Space(10);

            materialEditor.ShaderProperty(litProperties.noiseOn, StylesS.noiseOnText, 0);
            if (material.GetInt("_NoiseOn") == 1)
            {
                materialEditor.TexturePropertySingleLine(StylesS.noiseMapText, litProperties.noiseMap, litProperties.noiseIntensity);

                materialEditor.TextureScaleOffsetProperty(litProperties.noiseMap);
                var noiseMapTiling = litProperties.additionalMap.textureScaleAndOffset;
                material.SetTextureScale("_AdditionalMap", new Vector2(noiseMapTiling.x, noiseMapTiling.y));
                material.SetTextureOffset("_AdditionalMap", new Vector2(noiseMapTiling.z, noiseMapTiling.w));


            }

            EditorGUILayout.Space(10);

            materialEditor.ShaderProperty(litProperties.vertexPaint, StylesS.vertexPaintText, 0);

        }


    // material main surface inputs
    public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            SnowLitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {

            //눈 쉐이더 관련
            EditorGUILayout.Space();
            DrawSnowInputs(material);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Other Option", MessageType.None);

            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                materialEditor.ShaderProperty(litProperties.highlights, SnowLitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, SnowLitGUI.Styles.reflectionsText);
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Blend", (float)blendMode);

            material.SetFloat("_Surface", (float)surfaceType);
            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)SnowLitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)SnowLitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
    }
}
