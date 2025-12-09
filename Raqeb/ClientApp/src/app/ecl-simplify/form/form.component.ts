import { Component } from '@angular/core';
import { SwaggerClient, FileParameter, ApiResponseOfString } from '../../shared/services/Swagger/SwaggerClient.service';
import Swal from 'sweetalert2';

@Component({
    selector: 'app-form',
    templateUrl: './form.component.html',
    styleUrl: './form.component.scss'
})
export class FormComponent {
    // For PD type (two files)
    pdFile: File | null = null;
    macroFile: File | null = null;
    uploading: boolean = false;

    constructor(private swaggerClient: SwaggerClient) { }

    // PD file selection
    onPDFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            this.pdFile = file;
        }
    }

    // Macro file selection
    onMacroFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            this.macroFile = file;
        }
    }

    // Check if both files are selected
    canUpload(): boolean {
        return !!(this.pdFile && this.macroFile);
    }

    async uploadFile() {
        if (!this.canUpload()) {
            Swal.fire({
                icon: 'warning',
                title: 'تنبيه',
                text: 'الرجاء اختيار كلا الملفين (Data و Macro)',
                confirmButtonText: 'حسناً',
                confirmButtonColor: '#ffc107'
            });
            return;
        }

        this.uploading = true;

        try {
            const pdFileParam: FileParameter = {
                data: this.pdFile!,
                fileName: this.pdFile!.name
            };

            const macroFileParam: FileParameter = {
                data: this.macroFile!,
                fileName: this.macroFile!.name
            };

            this.swaggerClient.apiPDImportPost(pdFileParam, macroFileParam).subscribe(
                (response: ApiResponseOfString) => {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'تم بنجاح',
                            text: 'تم رفع ملفات Data بنجاح',
                            confirmButtonText: 'حسناً',
                            confirmButtonColor: '#28a745'
                        });
                        this.resetFileInputs();
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'خطأ',
                            text: response.message || 'حدث خطأ أثناء رفع الملفات',
                            confirmButtonText: 'حسناً',
                            confirmButtonColor: '#dc3545'
                        });
                    }
                },
                (error) => {
                    console.error('Upload failed', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'خطأ',
                        text: 'حدث خطأ أثناء رفع الملفات',
                        confirmButtonText: 'حسناً',
                        confirmButtonColor: '#dc3545'
                    });
                }
            ).add(() => {
                this.uploading = false;
            });
        } catch (error) {
            console.error('Error during upload', error);
            Swal.fire({
                icon: 'error',
                title: 'خطأ',
                text: 'حدث خطأ أثناء رفع الملفات',
                confirmButtonText: 'حسناً',
                confirmButtonColor: '#dc3545'
            });
            this.uploading = false;
        }
    }

    private resetFileInputs() {
        this.pdFile = null;
        this.macroFile = null;

        // Reset all file inputs
        const fileInputs = document.querySelectorAll('input[type="file"]') as NodeListOf<HTMLInputElement>;
        fileInputs.forEach(input => {
            if (input) input.value = '';
        });
    }
}
