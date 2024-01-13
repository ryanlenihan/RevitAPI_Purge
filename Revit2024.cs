//
//Code is written for use as a C# macro
//
public void ListAndPurgeUnusedMaterialAssets()
{
    UIDocument uidoc = this.ActiveUIDocument;
    Document doc = uidoc.Document;

    // Get all unused elements in the document
    ISet<ElementId> unusedElementIds = doc.GetUnusedElements(new HashSet<ElementId>());

    List<string> unusedAssetNames = new List<string>();
    List<ElementId> unusedAssetIds = new List<ElementId>();

    // Process each unused element and filter for material assets and appearance assets
    foreach (ElementId elementId in unusedElementIds)
    {
        Element elem = doc.GetElement(elementId);
        if (elem != null)
        {
            // Check if the element is an AppearanceAssetElement
            if (elem is AppearanceAssetElement)
            {
                unusedAssetNames.Add(elem.Name + " (Appearance)");
                unusedAssetIds.Add(elementId);
            }
            // Check if the element is in the Material Assets category
            else if (elem.Category != null && elem.Category.Name == "Material Assets")
            {
                unusedAssetNames.Add(elem.Name + " (Material Asset)");
                unusedAssetIds.Add(elementId);
            }
        }
    }

    // Sort the list of names for better readability
    unusedAssetNames.Sort();

    // Display the names of unused assets in a Task Dialog
    string dialogContent = string.Join(Environment.NewLine, unusedAssetNames);
    TaskDialog.Show("Unused Assets", dialogContent);

    // Confirm with the user before deletion
    TaskDialogResult result = TaskDialog.Show("Confirm Deletion", "Are you sure you want to delete the following unused assets?\n\n" + dialogContent, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

    if (result == TaskDialogResult.Yes)
    {
        // Delete the unused assets
        using (Transaction t = new Transaction(doc, "Delete Unused Assets"))
        {
            t.Start();
            try
            {
                doc.Delete(unusedAssetIds);
                t.Commit();
            }
            catch (Exception ex)
            {
                // Handle the exception (could be element can't be deleted, etc.)
                t.RollBack();
                TaskDialog.Show("Error", "An error occurred while deleting assets: " + ex.Message);
            }
        }
    }
}