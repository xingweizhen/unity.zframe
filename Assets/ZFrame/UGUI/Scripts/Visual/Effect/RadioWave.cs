using System;
using UnityEngine;
using ZFrame.UGUI;

public class RadioWave : MonoBehaviour
{

	// 控制频率
	public float F1 = 0.7f;
	public float F2 = 0.16f;
	public float F3 = 0.05f;
	
	// 控制移动速度
	public float T1 = 1.82f;
	public float T2 = 2.32f;
	public float T3 = -4.3f;
	
	// 控制初始相位，主要用于调试波形
	public float O1 = 1.570796f;
	public float O2 = 1.570796f;
	public float O3 = 1.570796f;
	
	// 波形高度
	public float H = 50;
	
	// 波形宽度
	public float W = 3;
	
	// 整体控制频率，对应游戏中的频率旋钮
	public float F = 3.0f;
	

	private Vector2[] _points = new Vector2[200];
	private UILineRenderer _renderer;
	
	// Use this for initialization
	void Start ()
	{
		_renderer = gameObject.GetComponent<UILineRenderer>();
		_renderer.Points = _points;
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < _points.Length; ++i)
		{
			var p1 = (float) Math.Sin((Time.time * T1 + i) * F1 * F + O1);
			var p2 = (float) Math.Sin((Time.time * T2 + i) * F2 * F + O2);
			var p3 = (float) Math.Sin(Time.time * T3 + i * F3 + O3);
			var l = _points.Length / 2.0f;
			var p4 = Mathf.SmoothStep(1, 0, Math.Abs(i - l) / l);

			_points[i] = new Vector2(i * W, p1 * p2 * p3* p4 * H);
		}
		
		_renderer.SetVerticesDirty();
	}
}
