//
//Code is written for use as a C# macro
//
private ICollection<ElementId> GetUnusedAssets(Document doc, string methodName)
{
	MethodInfo method = typeof(Document).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
	if (method != null)
	{
		return (ICollection<ElementId>)method.Invoke(doc, null);
	}
	return new List<ElementId>();
}

private void AddUnusedAssets(Document doc, ICollection<ElementId> elementIds, string assetType, List<string> names, List<ElementId> ids)
{
	foreach (var id in elementIds)
	{
		Element elem = doc.GetElement(id);
		if (elem != null)
		{
			names.Add(elem.Name + " (" + assetType + ")");
			ids.Add(id);
		}
	}
}

public void PurgeUnusedMaterialAssets()
{
	UIDocument uidoc = this.ActiveUIDocument;
	Document doc = uidoc.Document;
	
	List<string> unusedAssetNames = new List<string>();
	List<ElementId> unusedAssetIds = new List<ElementId>();

	try
	{
		// Retrieve unused assets using reflection
		AddUnusedAssets(doc, GetUnusedAssets(doc, "GetUnusedMaterials"), "Material", unusedAssetNames, unusedAssetIds);
		AddUnusedAssets(doc, GetUnusedAssets(doc, "GetUnusedAppearances"), "Appearance", unusedAssetNames, unusedAssetIds);
		AddUnusedAssets(doc, GetUnusedAssets(doc, "GetUnusedStructures"), "Structure", unusedAssetNames, unusedAssetIds);
		AddUnusedAssets(doc, GetUnusedAssets(doc, "GetUnusedThermals"), "Thermal", unusedAssetNames, unusedAssetIds);

		// Sort the list of names for better readability
		unusedAssetNames.Sort();

		// Display the list to the user and ask for confirmation before deletion
		string dialogContent = string.Join(Environment.NewLine, unusedAssetNames);
		TaskDialogResult result = TaskDialog.Show("Confirm Deletion", "Are you sure you want to delete the following unused assets?\n\n" + dialogContent, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

		if (result == TaskDialogResult.Yes)
		{
			// Delete the unused assets within a transaction
			using (Transaction t = new Transaction(doc, "Delete Unused Assets"))
			{
				t.Start();
				doc.Delete(unusedAssetIds);
				t.Commit();
			}
		}
	}
	catch (Exception ex)
	{
		TaskDialog.Show("Error", "An error occurred: " + ex.Message);
	}

}