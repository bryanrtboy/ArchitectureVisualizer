using UnityEngine;

public class AttenuationInterpolator {

	public void Init(float interpolationFrames)
    {
        numInterpFrames = interpolationFrames;
        startValue = .0f;
        endValue = .0f;
        currentValue = .0f;
        frameIndex = .0f;
        isInit = true;
        isDone = false;
	}
	
    public void Reset()
    {
        isInit = true;
    }

	public float Update()
    {
        if (isDone)
            return currentValue;
        else
        {
            frameIndex += 1.0f;
            float alpha = frameIndex / numInterpFrames;
            if (alpha >= 1.0f)
            {
                isDone = true;
                currentValue = endValue;
            }
            else
                currentValue = Mathf.Lerp(startValue, endValue, alpha);

            return currentValue;
        }
    }

    public void Set(float value)
    {
        if (isInit || numInterpFrames == .0f)
        {
            isInit = false;
            currentValue = value;
            startValue = value;
            endValue = value;
            frameIndex = numInterpFrames;
            if (numInterpFrames == .0f)
                isDone = true;
            else
                isDone = false;
        }
        else
        {
            startValue = currentValue;
            endValue = value;
            frameIndex = .0f;
            isDone = false;
        }
    }

    public float Get()
    {
        return currentValue;
    }

    float frameIndex;
    float numInterpFrames;

    float currentValue;
    float startValue;
    float endValue;

    bool isDone;
    bool isInit;
}
