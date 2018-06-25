using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gallery : MonoBehaviour {
	private int currentImage = -1;
	public Sprite[] imageArray;
	public Image displayedImage;

	// Use this for initialization
	void Start () {
		InvokeRepeating("ChangeGalleryImage", 0.0f, 5f);
	}
	
	void ChangeGalleryImage(){
		if (currentImage < imageArray.Length -1){
			currentImage++;
		}
		else{ 
			currentImage = 0;
		}
		displayedImage.sprite = imageArray[currentImage];
	}
}
