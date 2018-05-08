using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScorePopup : MonoBehaviour {

	static Camera camera;
	[SerializeField] TextMesh scoreText;

	float startTime;
	float lifetime = 1;

	[SerializeField] AnimationCurve xCurve;
	float xmult = 1;
	[SerializeField] AnimationCurve yCurve;
	float ymult = 1;	

	Vector3 startPos;

	[SerializeField] Color posColor = Color.green;
	[SerializeField] Color negColor = Color.red;

	void Start () {
		if (camera == null)
			camera = FindObjectOfType<Camera>();

		startTime = Time.realtimeSinceStartup;

		startPos = transform.localPosition;

		xmult = Random.Range(-1.5f, 1.5f);
		ymult = Random.Range(0.5f, 1.5f);
	}

	public void SetScore(float s) {
		int score = Mathf.FloorToInt(s);

		if (score == 0)
			Destroy(gameObject);

		scoreText.text = (score > 0 ? "+" : "") + score.ToString();
		scoreText.color = score > 0 ? posColor : negColor;

		transform.localScale = Vector3.one * Mathf.Clamp(0.5f + (Mathf.Abs(s) / 5000f), 0.5f, 1.5f);
	}
	
	void Update () {
		float life = Time.realtimeSinceStartup - startTime;

		transform.localPosition = startPos + new Vector3(xCurve.Evaluate(life / lifetime) * xmult, yCurve.Evaluate(life / lifetime) * ymult, 0);
		transform.LookAt(camera.transform.position);

		if (life > lifetime)
			Destroy(gameObject);
	}
}
