export default class EventBus {
  private static callbackId: number = 0;
  private static callbacksPerEventId: { [key: string]: Array<RegisteredCallback> } = {};

	public static on(eventId: string, callback: Function): CallbackUnregisterShortcut {
    const callbacksForEventId = this.callbacksPerEventId[eventId] || [];
    this.callbackId = this.callbackId + 1;
    const cId = this.callbackId;
    callbacksForEventId.push({
      id: cId,
      callback: callback
    });
    this.callbacksPerEventId[eventId] = callbacksForEventId;
    return {
      id: cId,
      unregister: () => this.off(cId)
    };
	}

  public static off(callbackId: number): void {
    let found = false;
    Object.keys(this.callbacksPerEventId).forEach(x => {
      const index = this.callbacksPerEventId[x].findIndex(c => c.id == callbackId);
      if (index != -1) {
        found = true;
        this.callbacksPerEventId[x].splice(index, 1); 
      }
    });
    if (!found)
    {
      console.warn(`Attempted to unsubscribe from event with callback id ${callbackId} that wasn't found.`);
    }
  }

	public static notify(eventId: string, data: any | null = null): void {
    const callbacks = this.callbacksPerEventId[eventId] || [];
    callbacks.forEach(x => x.callback(data));
	}
}

interface RegisteredCallback
{
  id: number;
  callback: Function;
}

export interface CallbackUnregisterShortcut
{
  id: number;
  unregister: Function;
}
