export function isValidCpf(value: string): boolean {
  if (!value?.trim()) return false;

  const digits = value.replace(/\D/g, '');
  if (digits.length !== 11) return false;
  if (digits.split('').every((c) => c === digits[0])) return false;

  const numbers = digits.split('').map((c) => parseInt(c, 10));

  let sum1 = 0;
  for (let i = 0; i < 9; i++) {
    sum1 += numbers[i]! * (10 - i);
  }
  const remainder1 = sum1 % 11;
  const checkDigit1 = remainder1 < 2 ? 0 : 11 - remainder1;
  if (numbers[9] !== checkDigit1) return false;

  let sum2 = 0;
  for (let i = 0; i < 10; i++) {
    sum2 += numbers[i]! * (11 - i);
  }
  const remainder2 = sum2 % 11;
  const checkDigit2 = remainder2 < 2 ? 0 : 11 - remainder2;
  if (numbers[10] !== checkDigit2) return false;

  return true;
}
