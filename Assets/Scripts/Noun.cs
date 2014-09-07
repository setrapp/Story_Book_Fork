﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Noun : MonoBehaviour {
	public string id;
	public bool possibleSubject;
	public bool inInventory;
	public GameObject container;
	public GameObject outline;
	public List<ObjectVerb> objectVerbs;
	[HideInInspector]
	public List<ObjectVerb> objectDefaultVerbs;
	public int firstPage = -1;

	void Start () 
	{
		GameObject.FindGameObjectWithTag("Globals").GetComponent<PageManager>().allNouns.Add(this);

		if (container == null && transform.parent != null)
		{
			container = transform.parent.gameObject;
		}

		if (inInventory)
		{
			Inventory inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
			inventory.AddNoun(this);
		}

		if (objectVerbs == null)
		{
			objectVerbs = new List<ObjectVerb>();
		}
		objectDefaultVerbs = new List<ObjectVerb>();
		ResetDefaultVerbsToCurrent();
	}

	public void DisplayInPage()
	{
		if (container != null)
		{
			container.SetActive(true);
		}
		else
		{
			if (!inInventory)
			{
				gameObject.SetActive(true);
			}
			if (outline != null)
			{
				outline.SetActive(true);
			}
		}
	}

	public void HideInPage()
	{
		if (container != null)
		{
			container.SetActive(false);
		}
		else
		{
			if (!inInventory)
			{
				gameObject.SetActive(false);
			}
			if (outline != null)
			{
				outline.SetActive(false);
			}
		}
	}

	private void ResetDefaultVerbsToCurrent() 
	{
		for (int i = 0; i < objectVerbs.Count; i++) 
		{
			if (i >= objectDefaultVerbs.Count)
			{
				objectDefaultVerbs.Add(new ObjectVerb(objectVerbs[i]));
			} 
			else
			{
				objectDefaultVerbs[i] = new ObjectVerb(objectVerbs[i]);
			}
			
		}
	}

	private void ResetVerbsToDefault()
	{
		objectVerbs.Clear();
		for (int i = 0; i < objectDefaultVerbs.Count; i++)
		{
			if (i >= objectVerbs.Count)
			{
				objectVerbs.Add(new ObjectVerb(objectDefaultVerbs[i]));
			}
			else
			{
				objectVerbs[i] = new ObjectVerb(objectDefaultVerbs[i]);
			}
		}
	}

	public Verb FindObjectConnection(Noun obj)
	{
		Verb connection = null;
		for (int i = 0; i < objectVerbs.Count && connection == null; i++)
		{
			if (objectVerbs[i].obj == obj)
			{
				connection = objectVerbs[i].verb;
			}
		}
		return connection;
	}

	public Verb FindObjectDefaultConnection(Noun obj)
	{
		Verb connection = null;
		for (int i = 0; i < objectDefaultVerbs.Count && connection == null; i++)
		{
			if (objectDefaultVerbs[i].obj == obj)
			{
				connection = objectDefaultVerbs[i].verb;
			}
		}
		return connection;
	}

	public Verb SetObjectConnection(Noun obj, Verb connection)
	{
		bool setConnection = false;
		for (int i = 0; i < objectVerbs.Count && !setConnection; i++)
		{
			if (objectVerbs[i].obj == obj)
			{
				objectVerbs[i].verb = connection;
				setConnection = true;
			}
		}
		return connection;
	}

	private void SnapToOutline()
	{
		Vector3 newPos = outline.transform.position;
		newPos.z = outline.transform.position.z + 1;
		transform.position = newPos;
	}

	public void MouseUp()
	{
		Inventory inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
		if (inInventory)
		{
			inventory.RemoveNoun(this);
			SnapToOutline();
			inInventory = false;
		}
		else
		{
			inventory.AddNoun(this);
			inInventory = true;
		}

		// TODO: Rather than checking page definitions, this should be based off of connected objects.
		// Modify connected objects on the following page, depending on existence in page or inventory.
		PageManager pageManager = GameObject.FindGameObjectWithTag("Globals").GetComponent<PageManager>();
		if (pageManager.PageIndex < pageManager.pages.Count - 1)
		{
			NounPlacement placeInPage = pageManager.FindNounPlacement(pageManager.PageIndex, this);
			if (placeInPage != null && placeInPage.startAction != null && placeInPage.startAction.obj != null && placeInPage.startAction.verb != null)
			{
				NounPlacement objPlaceInNext = pageManager.FindNounPlacement(pageManager.PageIndex + 1, placeInPage.startAction.obj);
				if (objPlaceInNext != null && objPlaceInNext.startAction != null && objPlaceInNext.startAction.verb != null)
				{
					VerbTag verbEffect = placeInPage.startAction.verb.FindRelatedTag(objPlaceInNext.startAction.verb.type);
					if (verbEffect != null)
					{
						if (inInventory && objPlaceInNext.startAction.modifiedBy == this)
						{
							objPlaceInNext.startAction.modifier = VerbTag.Relationship.NONE;
							objPlaceInNext.startAction.modifiedBy = null;
						}
						else if (!inInventory && objPlaceInNext.startAction.modifiedBy == null)
						{
							objPlaceInNext.startAction.modifier = verbEffect.relationship;
							objPlaceInNext.startAction.modifiedBy = this;
						}
					}
				}
			}
		}
	}
}

[System.Serializable]
public class ObjectVerb
{
	public Noun obj;
	public Verb verb;
	public VerbTag.Relationship modifier;
	public Noun modifiedBy;
	
	public ObjectVerb(Noun obj, Verb verb)
	{
		this.obj = obj;
		this.verb = verb;
	}
	
	public ObjectVerb(ObjectVerb original)
	{
		this.obj = original.obj;
		this.verb = original.verb;
	}
}
