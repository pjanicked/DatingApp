import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { AuthService } from 'src/app/_services/auth.service';
import { environment } from 'src/environments/environment';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() getUserPhotoChange = new EventEmitter<string>();
  uploader: FileUploader;
  hasBaseDropZoneOver: boolean;
  baseUrl = environment.apiUrl;
  currentMain: Photo;

  constructor(private authService: AuthService, private userService: UserService,
      private alertify: AlertifyService) { }

  ngOnInit() {
    this.photoInitializer();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  photoInitializer() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      allowedFileType: ['image'],
      isHTML5: true,
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => { file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          description: res.description,
          dateAdded: res.dateAdded,
          isMain: res.isMain
        };

        this.photos.push(photo);
      }
    };
  }

  setMain(photo: Photo) {
    return this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(() => {
      this.currentMain = this.photos.filter(p => p.isMain)[0];
      this.currentMain.isMain = false;
      photo.isMain = true;
      // this.getUserPhotoChange.emit(photo.url);
      this.authService.changeMemberPhoto(photo.url);
      this.authService.currentUser.photoUrl = photo.url;
      localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
      console.log('Success');
    }, error => {
      this.alertify.error(error);
    });
  }

  deletePhoto(photo: Photo) {
     this.alertify.confirm('Are you sure you want to delete?', () =>
      this.userService.deletePhoto(this.authService.decodedToken.nameid, photo.id).subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id === photo.id), 1);
        this.alertify.success('Photo Deleted successfully');
    }, error => {
      this.alertify.error(error);
    }));
  }

}
