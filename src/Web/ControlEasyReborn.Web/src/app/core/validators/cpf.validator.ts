import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { isValidCpf } from '../utils/cpf.util';

export function cpfValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) {
      return null;
    }
    return isValidCpf(String(value)) ? null : { invalidCpf: true };
  };
}
