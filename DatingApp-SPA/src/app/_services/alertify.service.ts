import { Injectable } from '@angular/core';
declare let alertify: any;

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

constructor() { }

confirm(message: any, okCallback: () => any) {
  alertify.confirm(message, function(e) {
    if (e) {
      okCallback();
    } else {}
  });
}

success(message: any) {
  alertify.success(message);
}

error(message: any) {
  alertify.error(message);
}

warning(message: any) {
  alertify.warning(message);
}

message(message: any) {
  alertify.message(message);
}

}
