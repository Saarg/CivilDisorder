using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour {

	[SerializeField] SettingsHolder settings;

	[SerializeField] Text resolutionText;
	[SerializeField] Button nextResolutionButton;
	[SerializeField] Button prevResolutionButton;
	[SerializeField] Toggle fullscreenToggle;
	[SerializeField] Toggle vsyncToggle;
	[SerializeField] Slider carVolumeSlider;
	[SerializeField] Slider envVolumeSlider;
	[SerializeField] Slider musicVolumeSlider;
	
	// Update is called once per frame
	void Update () {
		resolutionText.text = Screen.width + "x" + Screen.height;

		if (settings.Resolution <= 0) {
			prevResolutionButton.gameObject.SetActive(false);
		} else
		{
			prevResolutionButton.gameObject.SetActive(true);			
		}

		if (settings.Resolution >= Screen.resolutions.Length-1 || settings.Resolution < 0) {
			nextResolutionButton.gameObject.SetActive(false);
		} else
		{
			nextResolutionButton.gameObject.SetActive(true);			
		}
	}
}
