using UnityEngine;

public class Node : MonoBehaviour
{
	public Color hoverColor;
	public Vector3 positionOffset;

	private GameObject turret;

	private Renderer rend;
	private MeshRenderer meshRend;
	private Color startColor;

	void Start()
	{
		rend = GetComponent<Renderer>();
		startColor = rend.material.color;
		meshRend = gameObject.GetComponent<MeshRenderer>();
		meshRend.enabled = false;
	}

	void OnMouseEnter()
	{
		rend.material.color = hoverColor;
	}

	void OnMouseExit()
	{
		rend.material.color = startColor;
	}

	public void BuildTurret()
	{
		if (turret != null || !meshRend.enabled)
		{
			Debug.Log("Can't build there! - TODO: Display on screen.");
			return;
		}

		GameObject turretToBuild = BuildManager.instance.GetTurretToBuild();
		turret = (GameObject)Instantiate(turretToBuild, transform.position + positionOffset, transform.rotation);
	}

    private void OnTriggerEnter(Collider other)
    {
		if (other.gameObject.CompareTag("Player"))
		{
			meshRend.enabled = true;
			other.gameObject.GetComponent<PlayerMovement3D>().SetCurrNode(this);
		}
	}

    private void OnTriggerExit(Collider other)
    {
		if (other.gameObject.CompareTag("Player"))
		{
			meshRend.enabled = false;
		}
	}
}