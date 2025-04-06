using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BankManager : MonoBehaviour {
    public GameObject customerPrefab;
    public Transform[] queuePositions;
    public Transform counterPosition;

    private Dictionary<string, GameObject> customerObjects = new();
    private List<string> waitingQueue = new();

    void Start() {
        TextAsset jsonFile = Resources.Load<TextAsset>("bank_log");
        BankEvent[] events = JsonUtility.FromJson<BankEventList>("{\"events\":" + jsonFile.text + "}").events;
        StartCoroutine(PlayEvents(events));
    }

    IEnumerator PlayEvents(BankEvent[] events) {
        float simStartTime = Time.time;

        foreach (var e in events.OrderBy(ev => ev.time)) {
            float delay = e.time - (Time.time - simStartTime);
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            HandleEvent(e);
        }
    }

    void HandleEvent(BankEvent e) {
        if (e.@event == "arrival") {
            GameObject customer = Instantiate(customerPrefab, queuePositions[waitingQueue.Count].position, Quaternion.identity);
            customer.name = e.customer;
            customerObjects[e.customer] = customer;
            waitingQueue.Add(e.customer);
        }
        else if (e.@event == "service_start") {
            string name = e.customer;
            int index = waitingQueue.IndexOf(name);
            if (index >= 0) waitingQueue.RemoveAt(index);

            if (customerObjects.TryGetValue(name, out GameObject customer)) {
                StartCoroutine(MoveTo(customer, counterPosition.position));
            }
        }
        else if (e.@event == "service_end" || e.@event == "renege") {
            if (customerObjects.TryGetValue(e.customer, out GameObject customer)) {
                Destroy(customer);
                customerObjects.Remove(e.customer);
            }
        }

        UpdateQueuePositions();
    }

    void UpdateQueuePositions() {
        for (int i = 0; i < waitingQueue.Count; i++) {
            string name = waitingQueue[i];
            if (customerObjects.TryGetValue(name, out GameObject customer)) {
                StartCoroutine(MoveTo(customer, queuePositions[i].position));
            }
        }
    }

    IEnumerator MoveTo(GameObject obj, Vector3 target) {
        float t = 0f;
        Vector3 start = obj.transform.position;
        while (t < 1f) {
            t += Time.deltaTime * 2;
            obj.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }
}
