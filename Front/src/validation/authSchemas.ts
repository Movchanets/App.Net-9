import * as yup from 'yup'
import i18n from '../i18n'

export const emailSchema = yup
  .string()
  .email(i18n.t('validation.email'))
  .required(i18n.t('validation.required'))

export const passwordSchema = yup
  .string()
  .min(6, i18n.t('validation.min_6'))
  .required(i18n.t('validation.required'))

export const nameSchema = yup
  .string()
  .min(2, i18n.t('validation.min_2'))
  .required(i18n.t('validation.required'))

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
  surname: nameSchema,
  password: passwordSchema,
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], i18n.t('validation.passwords_must_match'))
    .required(i18n.t('validation.required')),
})
export type RegisterFormValues = yup.InferType<typeof registerFormSchema>

export const forgotPasswordFormSchema = emailFormSchema
export type ForgotPasswordFormValues = EmailFormValues
