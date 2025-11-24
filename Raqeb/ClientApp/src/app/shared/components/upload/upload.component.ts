import { Component, Input } from '@angular/core';
import { SwaggerClient, FileParameter } from '../../../shared/services/Swagger/SwaggerClient.service';
import Swal from 'sweetalert2';

@Component({
    selector: 'app-upload',
    templateUrl: './upload.component.html',
    styleUrl: './upload.component.scss'
})
export class UploadComponent {
    @Input() HeaderTitle!: string;
    @Input() BtnTitle!: string;
    @Input() Type!: string;

    // For LGD type (single file)
    selectedFile: File | null = null;

    // For PD type (two files - both optional)
    pdFile: File | null = null;
    macroFile: File | null = null;

    uploading: boolean = false;

    constructor(private swaggerClient: SwaggerClient) { }

    // LGD file selection
    onFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            this.selectedFile = file;
        }
    }

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

    // Check if files are selected based on type
    isFilesSelected(): boolean {
        if (this.Type === 'PD') {
            return !!(this.pdFile || this.macroFile); // أي ملف من الاثنين
        }
        return !!this.selectedFile;
    }

    // Check if upload can proceed
    canUpload(): boolean {
        if (this.Type === 'PD') {
            return !!(this.pdFile || this.macroFile); // أي ملف من الاثنين
        }
        return !!this.selectedFile;
    }

    async uploadFile() {
        if (!this.canUpload()) {
            const message = this.Type === 'PD'
                ? 'الرجاء اختيار ملف واحد على الأقل'
                : 'الرجاء اختيار ملف أولاً';

            Swal.fire({
                icon: 'warning',
                title: 'تنبيه',
                text: message,
                confirmButtonText: 'حسناً',
                confirmButtonColor: '#ffc107'
            });
            return;
        }

        this.uploading = true;

        try {
            if (this.Type === 'PD') {
                // Handle PD upload with optional files
                const pdFileParam: FileParameter | null = this.pdFile ? {
                    data: this.pdFile,
                    fileName: this.pdFile.name
                } : null;

                const macroFileParam: FileParameter | null = this.macroFile ? {
                    data: this.macroFile,
                    fileName: this.macroFile.name
                } : null;

                this.swaggerClient.apiPDImportPost(pdFileParam, macroFileParam).subscribe(
                    (response) => {
                        if (response.success) {
                            // رسالة ديناميكية حسب الملفات المرفوعة
                            let successMessage = 'تم رفع ';
                            if (pdFileParam && macroFileParam) {
                                successMessage += 'ملفات PD و Macro بنجاح';
                            } else if (pdFileParam) {
                                successMessage += 'ملف PD بنجاح';
                            } else {
                                successMessage += 'ملف Macro بنجاح';
                            }

                            Swal.fire({
                                icon: 'success',
                                title: 'تم بنجاح',
                                text: successMessage,
                                confirmButtonText: 'حسناً',
                                confirmButtonColor: '#28a745'
                            });
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'خطأ',
                                text: response.message || 'حدث خطأ أثناء رفع الملفات',
                                confirmButtonText: 'حسناً',
                                confirmButtonColor: '#dc3545'
                            });
                        }
                        this.resetFileInput();
                    },
                    (error) => {
                        this.handleUploadError(error);
                    }
                ).add(() => {
                    this.uploading = false;
                });
            } else if (this.Type === 'LGD') {
                // Handle LGD upload with single file
                const fileParam: FileParameter = {
                    data: this.selectedFile!,
                    fileName: this.selectedFile!.name
                };

                this.swaggerClient.apiLGDUploadPost(fileParam).subscribe(
                    (response) => {
                        Swal.fire({
                            icon: 'success',
                            title: 'تم بنجاح',
                            text: 'تم رفع ملف LGD بنجاح',
                            confirmButtonText: 'حسناً',
                            confirmButtonColor: '#28a745'
                        });
                        this.resetFileInput();
                    },
                    (error) => {
                        this.handleUploadError(error);
                    }
                ).add(() => {
                    this.uploading = false;
                });
            }
        } catch (error) {
            this.handleUploadError(error);
        }
    }

    private resetFileInput() {
        this.selectedFile = null;
        this.pdFile = null;
        this.macroFile = null;

        // Reset all file inputs
        const fileInputs = document.querySelectorAll('input[type="file"]') as NodeListOf<HTMLInputElement>;
        fileInputs.forEach(input => {
            if (input) input.value = '';
        });
    }

    private handleUploadError(error: any) {
        console.error('Upload failed', error);
        Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: 'حدث خطأ أثناء رفع الملف',
            confirmButtonText: 'حسناً',
            confirmButtonColor: '#dc3545'
        });
        this.uploading = false;
    }


    removePDFile(event: Event) {
        event.stopPropagation();
        this.pdFile = null;
        const input = document.getElementById('pdFileInput') as HTMLInputElement;
        if (input) input.value = '';
    }

    removeMacroFile(event: Event) {
        event.stopPropagation();
        this.macroFile = null;
        const input = document.getElementById('macroFileInput') as HTMLInputElement;
        if (input) input.value = '';
    }

}