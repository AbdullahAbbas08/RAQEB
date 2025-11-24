import { Component } from '@angular/core';
import { SwaggerClient, FileParameter } from '../../../shared/services/Swagger/SwaggerClient.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-second-page',
  templateUrl: './second-page.component.html',
  styleUrls: ['./second-page.component.scss']
})
export class SecondPageComponent {
  selectedFile: File | null = null;
  uploading: boolean = false;

  constructor(private swaggerClient: SwaggerClient) {}

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  async uploadFile() {
    if (!this.selectedFile) {
      alert('Please select a file first');
      return;
    }

    this.uploading = true;
    try {
      const fileParam: FileParameter = {
        data: this.selectedFile,
        fileName: this.selectedFile.name
      };

      this.swaggerClient.apiLGDUploadPost(fileParam).subscribe(
        (response) => {
          console.log('Upload successful', response);
           Swal.fire({
                  icon: 'success',
                  title: 'تم بنجاح',
                  text: '  بنجاح LGD تم رفع الملف وحساب ',
                  confirmButtonText: 'حسناً',
                  confirmButtonColor: '#28a745'
                });
          this.selectedFile = null;
          // Reset the file input
          const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
          if (fileInput) fileInput.value = '';
        },
        (error) => {
          console.error('Upload failed', error);
          alert('Upload failed: ' + error.message);
        }
      ).add(() => {
        this.uploading = false;
      });
    } catch (error) {
      console.error('Error during upload', error);
      alert('An error occurred during upload');
      this.uploading = false;
    }
  }
}
