using UnityEngine;

public class Recoil : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 recoilGoal = new Vector3(0, 60, 0), restRotation;
    public float recoilSpeed;
    public float restSpeed;
    public bool applyRecoil;

    private void LateUpdate()
    {
        Quaternion to = Quaternion.Euler(restRotation);
        float speed = restSpeed;

        if (applyRecoil)
        {
            to = Quaternion.Euler(recoilGoal);
            speed = recoilSpeed;
        }

        Quaternion rotation = Quaternion.RotateTowards(transform.localRotation, to, speed * Time.deltaTime);
        transform.localRotation = rotation;
    }
}
