[System.Serializable]
public class BankEvent {
    public float time;
    public string @event;
    public string customer;
}

[System.Serializable]
public class BankEventList {
    public BankEvent[] events;
}
