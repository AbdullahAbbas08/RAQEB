import { Component } from '@angular/core';
import { SwaggerClient, FileParameter } from '../../../shared/services/Swagger/SwaggerClient.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html',
  styleUrl: './form.component.scss'
})
export class FormComponent {
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

      this.swaggerClient.apiEclUploadPost(fileParam).subscribe(
        (response) => {
          console.log('Upload successful', response);
           Swal.fire({
                  icon: 'success',
                  title: 'تم بنجاح',
                  text: '  بنجاح ECL تم رفع الملف وحساب ',
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
