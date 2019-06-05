using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Settings
{
    public class ModelProcessSettings : AssetProcessSettings
    {
        public enum MaterialImportMode
        {
            ImportWithoutAnimation, AlwaysImport, NeverImport,
        }

        [System.Flags]
        public enum Prop
        {
            MeshCompression = 1,
            IsReadable = 1 << 1,
            OptimizeMesh = 1 << 2,
            ImportBlendShapes = 1 << 3,
            WeldVertices = 1 << 4,
            ImportNormals = 1 << 5,
            NormalCalculationMode = 1 << 6,
            NormalSmoothingAngle = 1 << 7,
            ImportTangents = 1 << 8,

            AnimationType = 1 << 9,
            AnimationCompression = 1 <<10,
            OptimizeGameObjects = 1 << 11,
            ImportConstraints = 1 << 12,

            ImportMaterials = 1 << 13,
        }

        [System.Serializable]
        protected class Settings : AbstractSettings
        {
            public ModelImporterMeshCompression MeshCompression;
            public bool IsReadable;
            public bool OptimizeMesh;
            public bool ImportBlendShapes;
            public bool WeldVertices;
            public ModelImporterNormals ImportNormals;
            public ModelImporterNormalCalculationMode NormalCalculationMode;
            public float NormalSmoothingAngle;
            public ModelImporterTangents ImportTangents;

            public ModelImporterAnimationType AnimationType;
            public ModelImporterAnimationCompression AnimationCompression;
            public bool OptimizeGameObjects;
            public bool ImportConstraints;
            public MaterialImportMode ImportMaterials;

            public void OnPreprocess(ModelImporter mi)
            {
                if (m_Disable) return;

                if (ContainsAsset(folders, mi.assetPath)) {
                    if (ContainsFlag(flags, (int)Prop.MeshCompression)) mi.meshCompression = MeshCompression;
                    if (ContainsFlag(flags, (int)Prop.IsReadable)) mi.isReadable = IsReadable;
                    if (ContainsFlag(flags, (int)Prop.OptimizeMesh)) mi.optimizeMesh = OptimizeMesh;
                    if (ContainsFlag(flags, (int)Prop.ImportBlendShapes)) mi.importBlendShapes = ImportBlendShapes;
                    if (ContainsFlag(flags, (int)Prop.WeldVertices)) mi.weldVertices = WeldVertices;
                    if (ContainsFlag(flags, (int)Prop.ImportNormals)) mi.importNormals = ImportNormals;
                    if (ContainsFlag(flags, (int)Prop.NormalCalculationMode)) mi.normalCalculationMode = NormalCalculationMode;
                    if (ContainsFlag(flags, (int)Prop.NormalSmoothingAngle)) mi.normalSmoothingAngle = NormalSmoothingAngle;
                    if (ContainsFlag(flags, (int)Prop.ImportTangents)) mi.importTangents = ImportTangents;
                    if (ContainsFlag(flags, (int)Prop.OptimizeGameObjects)) mi.optimizeGameObjects = OptimizeGameObjects;
                    if (ContainsFlag(flags, (int)Prop.AnimationType)) mi.animationType = AnimationType;

                    var hasAni = mi.assetPath.IndexOf('@') > 0;
                    mi.importAnimation = hasAni;
                    if (hasAni) {
#if UNITY_2018_2
                        if (ContainsFlag(flags, (int)Prop.ImportConstraints)) mi.importConstraints = ImportConstraints;
#endif
                        if (ContainsFlag(flags, (int)Prop.AnimationCompression)) mi.animationCompression = AnimationCompression;
                    } else {
#if UNITY_2018_2
                        mi.importConstraints = false;
#endif
                    }

                    if (ContainsFlag(flags, (int)Prop.ImportMaterials)) {
                        mi.importMaterials = ImportMaterials == MaterialImportMode.AlwaysImport
                            || (ImportMaterials == MaterialImportMode.ImportWithoutAnimation && !hasAni);

                    }
                }
            }

            public void OnPostprocess(GameObject go)
            {

            }
        }

        [SerializeField, HideInInspector] private List<Settings> m_SettingsList;

        private Prop m_Props;
        public override System.Enum props {
            get { return m_Props; }
            set { m_Props = (Prop)value; }
        }

        public override void OnPreprocess(AssetImporter ai)
        {
            var ti = (ModelImporter)ai;
            foreach (var setting in m_SettingsList) {
                setting.OnPreprocess(ti);
            }
        }

        public override void OnPostprocess(Object obj)
        {
            var go = (GameObject)obj;
            foreach (var setting in m_SettingsList) {
                setting.OnPostprocess(go);
            }
        }
    }
}
