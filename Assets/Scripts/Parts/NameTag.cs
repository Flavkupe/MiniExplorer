using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class NameTag : MonoBehaviour {

	private IHasName owner;

	private TextMesh textMesh;

	// Use this for initialization
	void Start () {
		this.owner = this.transform.parent.GetComponent(typeof(IHasName)) as IHasName;
		this.textMesh = this.GetComponent<TextMesh>();
		this.RefreshName(); 
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void RefreshName() 
	{
		if (this.owner != null && this.textMesh != null)  
		{
			this.textMesh = this.GetComponent<TextMesh>();
			this.textMesh.text = this.owner.GetName();
		}
	}
}
