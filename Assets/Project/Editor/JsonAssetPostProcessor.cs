// using UnityEditor;
//
// namespace Project.Editor
// {
// 	public class JsonAssetPostProcessor : AssetPostprocessor
// 	{
// 		static void OnPostprocessAllAssets(
// 				string[] importedAssets,
// 				string[] deletedAssets,
// 				string[] movedAssets,
// 				string[] movedFromAssetPaths)
// 		{
// 			// This method is called after the AssetDatabase is refreshed
// 			if (JsonToScriptableObjEditor.HasPendingFileCreation())
// 			{
// 				JsonToScriptableObjEditor.ProcessPendingFileCreation();
// 			}
// 		}
// 	}
// }