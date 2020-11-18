import {
  makeAutoObservable,
  onBecomeObserved,
  onBecomeUnobserved,
  runInAction,
} from "mobx";
import createFusionSubscription from "./Fusion";

export default class FusionStore {
  loading = true;
  error = null;
  data = null;
  cancel = null;

  unsubscribe = null;

  constructor(url, params, config) {
    makeAutoObservable(this);

    // HACK: using "loading" for now, but this really should work
    //       with any of loading/error/data
    onBecomeObserved(this, "loading", () => {
      this.initialize(url, params, config);
    });
    onBecomeUnobserved(this, "loading", () => {
      this.unsubscribe && this.unsubscribe();
      this.unsubscribe = null;
    });
  }

  initialize = (url, params, config) => {
    this.unsubscribe = createFusionSubscription(
      url,
      params,
      config
    ).subscribeToUpdates(({ loading, error, data, cancel } = {}) => {
      // using "unsubscribe" similarly to "isMounted" here to check
      // if we're still being observed
      this.unsubscribe &&
        runInAction(() => {
          this.loading = loading;
          this.error = error;
          this.data = data;
          this.cancel = cancel;
        });
    });
  };
}
