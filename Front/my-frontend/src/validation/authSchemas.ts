import * as yup from 'yup'

export const emailSchema = yup
  .string()
  .email('Невірний формат email')
  .required("Обов'язкове поле")

export const passwordSchema = yup
  .string()
  .min(6, 'Мінімум 6 символів')
  .required("Обов'язкове поле")

export const nameSchema = yup
  .string()
  .min(2, 'Мінімум 2 символи')
  .required("Обов'язкове поле")

export const emailFormSchema = yup.object({
  email: emailSchema,
})
export type EmailFormValues = yup.InferType<typeof emailFormSchema>

export const loginFormSchema = yup.object({
  password: passwordSchema,
})
export type LoginFormValues = yup.InferType<typeof loginFormSchema>

export const registerFormSchema = yup.object({
  name: nameSchema,
  password: passwordSchema,
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], 'Паролі не співпадають')
    .required("Обов'язкове поле"),
})
export type RegisterFormValues = yup.InferType<typeof registerFormSchema>

export const forgotPasswordFormSchema = emailFormSchema
export type ForgotPasswordFormValues = EmailFormValues
