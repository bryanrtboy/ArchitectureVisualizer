using UnityEngine;
using System.Collections;

using VTL.IO;

public class DictReaderTest : MonoBehaviour
{
	// specify the location of your csv file relative to a Resources folder
	// leave off the extension
	public string resourceLocation = "people/data";

	void Start ()
	{
		DictReader dictReader = new DictReader (resourceLocation);

		foreach (var row in dictReader)
			Debug.Log (row ["FIRSTNAME"] + ", " +
			row ["LASTNAME"] + ", " +
			row ["AGE"] + ", " +
			row ["LOCATION"] + ", " +
			row ["INCOME"]);


		Debug.Log (dictReader);
	}
}
