<template>
    <div class="profile-page">
        <div class="profile-card">
            <div class="profile-header">
                <div class="avatar-wrap">
                    <div class="avatar">
                        {{ displayName.charAt(0).toUpperCase() }}
                    </div>
                    <div class="status-dot" :class="{ active: isActive }"></div>
                </div>

                <div class="profile-title">
                    <h2>{{ displayName }}</h2>
                    <p>{{ email || "No email" }}</p>
                    <div class="role-chip">{{ role || "User" }}</div>
                </div>
            </div>

            <div class="profile-body">
                <div class="section-card">
                    <div class="section-head">
                        <div>
                            <div class="section-title">Profile Information</div>
                            <p class="section-subtitle">
                                Keep your account information up to date.
                            </p>
                        </div>

                        <button class="save-btn" @click="saveChanges" :disabled="loadingProfile">
                            <span v-if="loadingProfile">Saving...</span>
                            <span v-else>Save Changes</span>
                        </button>
                    </div>

                    <div class="info-grid">
                        <div class="info-item">
                            <label>Employee Code</label>
                            <input :value="employeeCode" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Username</label>
                            <input :value="username" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Full Name</label>
                            <input v-model="fullName" type="text" placeholder="Enter full name" />
                        </div>

                        <div class="info-item">
                            <label>Email Account</label>
                            <input v-model="email" type="email" placeholder="Enter email address" />
                        </div>

                        <div class="info-item">
                            <label>Role</label>
                            <input :value="role" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Department</label>
                            <input :value="departmentName || 'No department'" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Manager Code</label>
                            <input :value="managerCode || 'No manager'" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Manager Name</label>
                            <input :value="managerName || 'No manager assigned'" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Manager Email</label>
                            <input :value="managerEmail || 'No manager email'" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Created At</label>
                            <input :value="formatDateTime(createdAt)" type="text" disabled />
                        </div>

                        <div class="info-item">
                            <label>Last Login</label>
                            <input :value="formatDateTime(lastLoginAt)" type="text" disabled />
                        </div>
                    </div>
                </div>

                <div class="section-card password-section">
                    <div class="section-head column">
                        <div class="section-title">Change Password</div>
                        <p class="section-subtitle">
                            Enter your current password, receive an OTP by email, then confirm the new password.
                        </p>
                    </div>

                    <div class="password-grid">
                        <div class="info-item">
                            <label>Current Password</label>
                            <div class="password-field">
                                <input v-model="currentPassword" :type="showCurrentPassword ? 'text' : 'password'"
                                    placeholder="Enter current password" />
                                <button type="button" class="toggle-password"
                                    @click="showCurrentPassword = !showCurrentPassword">
                                    <svg v-if="!showCurrentPassword" viewBox="0 0 24 24" fill="none">
                                        <path d="M2 12S5.5 5 12 5s10 7 10 7-3.5 7-10 7S2 12 2 12Z" stroke="currentColor"
                                            stroke-width="2" />
                                        <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" />
                                    </svg>
                                    <svg v-else viewBox="0 0 24 24" fill="none">
                                        <path d="M3 3L21 21" stroke="currentColor" stroke-width="2" />
                                        <path d="M10.5 10.7A3 3 0 0 0 13.3 13.5" stroke="currentColor"
                                            stroke-width="2" />
                                        <path d="M9.9 5.1A11 11 0 0 1 12 5c6.5 0 10 7 10 7a17.3 17.3 0 0 1-4.2 5.1"
                                            stroke="currentColor" stroke-width="2" />
                                        <path d="M6.2 6.2A17.7 17.7 0 0 0 2 12s3.5 7 10 7a10.7 10.7 0 0 0 3-.4"
                                            stroke="currentColor" stroke-width="2" />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <div class="info-item">
                            <label>New Password</label>
                            <div class="password-field">
                                <input v-model="newPassword" :type="showNewPassword ? 'text' : 'password'"
                                    placeholder="Enter new password" />
                                <button type="button" class="toggle-password"
                                    @click="showNewPassword = !showNewPassword">
                                    <svg v-if="!showNewPassword" viewBox="0 0 24 24" fill="none">
                                        <path d="M2 12S5.5 5 12 5s10 7 10 7-3.5 7-10 7S2 12 2 12Z" stroke="currentColor"
                                            stroke-width="2" />
                                        <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" />
                                    </svg>
                                    <svg v-else viewBox="0 0 24 24" fill="none">
                                        <path d="M3 3L21 21" stroke="currentColor" stroke-width="2" />
                                        <path d="M10.5 10.7A3 3 0 0 0 13.3 13.5" stroke="currentColor"
                                            stroke-width="2" />
                                        <path d="M9.9 5.1A11 11 0 0 1 12 5c6.5 0 10 7 10 7a17.3 17.3 0 0 1-4.2 5.1"
                                            stroke="currentColor" stroke-width="2" />
                                        <path d="M6.2 6.2A17.7 17.7 0 0 0 2 12s3.5 7 10 7a10.7 10.7 0 0 0 3-.4"
                                            stroke="currentColor" stroke-width="2" />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <div class="info-item">
                            <label>Confirm New Password</label>
                            <div class="password-field">
                                <input v-model="confirmPassword" :type="showConfirmPassword ? 'text' : 'password'"
                                    placeholder="Confirm new password" />
                                <button type="button" class="toggle-password"
                                    @click="showConfirmPassword = !showConfirmPassword">
                                    <svg v-if="!showConfirmPassword" viewBox="0 0 24 24" fill="none">
                                        <path d="M2 12S5.5 5 12 5s10 7 10 7-3.5 7-10 7S2 12 2 12Z" stroke="currentColor"
                                            stroke-width="2" />
                                        <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" />
                                    </svg>
                                    <svg v-else viewBox="0 0 24 24" fill="none">
                                        <path d="M3 3L21 21" stroke="currentColor" stroke-width="2" />
                                        <path d="M10.5 10.7A3 3 0 0 0 13.3 13.5" stroke="currentColor"
                                            stroke-width="2" />
                                        <path d="M9.9 5.1A11 11 0 0 1 12 5c6.5 0 10 7 10 7a17.3 17.3 0 0 1-4.2 5.1"
                                            stroke="currentColor" stroke-width="2" />
                                        <path d="M6.2 6.2A17.7 17.7 0 0 0 2 12s3.5 7 10 7a10.7 10.7 0 0 0 3-.4"
                                            stroke="currentColor" stroke-width="2" />
                                    </svg>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="password-rules">
                        <div :class="{ ok: newPassword.length >= 6 }">
                            At least 6 characters
                        </div>
                        <div :class="{ ok: newPassword && confirmPassword && newPassword === confirmPassword }">
                            Password confirmation matches
                        </div>
                    </div>

                    <div class="otp-actions">
                        <button class="otp-btn" @click="handleSendOtp" :disabled="loadingOtp">
                            {{ loadingOtp ? "Sending OTP..." : "Send OTP" }}
                        </button>
                    </div>

                    <transition name="fade-slide">
                        <div v-if="otpSent" class="otp-panel">
                            <div class="otp-title">OTP Verification</div>
                            <p class="otp-subtitle">
                                Enter the 6-digit OTP sent to your email.
                            </p>

                            <div class="otp-boxes">
                                <input v-for="(digit, index) in otpDigits" :key="index"
                                    :ref="el => setOtpRef(el, index)" v-model="otpDigits[index]" type="text"
                                    maxlength="1" class="otp-input" @input="handleOtpInput(index, $event)"
                                    @keydown.backspace="handleOtpBackspace(index, $event)" />
                            </div>

                            <div class="otp-confirm-row">
                                <button class="confirm-btn" @click="handleConfirmPasswordChange"
                                    :disabled="loadingConfirm">
                                    {{ loadingConfirm ? "Confirming..." : "Confirm Change Password" }}
                                </button>
                            </div>
                        </div>
                    </transition>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue"
import {
    getMyProfile,
    updateMyProfile,
    sendChangePasswordOtp,
    confirmChangePassword
} from "@/services/profileService"

const loadingProfile = ref(false)
const loadingOtp = ref(false)
const loadingConfirm = ref(false)

const employeeCode = ref("")
const fullName = ref("")
const email = ref("")
const username = ref("")
const role = ref("")
const departmentName = ref("")
const managerCode = ref("")
const managerName = ref("")
const managerEmail = ref("")
const isActive = ref(false)
const createdAt = ref(null)
const lastLoginAt = ref(null)

const currentPassword = ref("")
const newPassword = ref("")
const confirmPassword = ref("")
const otpSent = ref(false)

const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

const otpDigits = ref(["", "", "", "", "", ""])
const otpRefs = ref([])

const displayName = computed(() => fullName.value || username.value || "User")

const setOtpRef = (el, index) => {
    if (el) otpRefs.value[index] = el
}

const getOtpCode = () => otpDigits.value.join("").trim()

const clearOtp = () => {
    otpDigits.value = ["", "", "", "", "", ""]
}

const handleOtpInput = (index, event) => {
    const value = event.target.value.replace(/\D/g, "")
    otpDigits.value[index] = value
    if (value && index < 5) otpRefs.value[index + 1]?.focus()
}

const handleOtpBackspace = (index, event) => {
    if (!otpDigits.value[index] && index > 0) {
        otpRefs.value[index - 1]?.focus()
    } else if (event.target.value) {
        otpDigits.value[index] = ""
    }
}

const formatDateTime = (value) => {
    if (!value) return "N/A"
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return "N/A"
    return date.toLocaleString("vi-VN")
}

const loadProfile = async () => {
    try {
        const res = await getMyProfile()
        const data = res.data

        employeeCode.value = data.employeeCode || ""
        fullName.value = data.fullName || ""
        email.value = data.email || ""
        username.value = data.username || localStorage.getItem("username") || ""
        role.value = data.role || localStorage.getItem("role") || ""
        departmentName.value = data.departmentName || ""
        managerCode.value = data.managerCode || ""
        managerName.value = data.managerName || ""
        managerEmail.value = data.managerEmail || ""
        isActive.value = !!data.isActive
        createdAt.value = data.createdAt || null
        lastLoginAt.value = data.lastLoginAt || null

        localStorage.setItem("fullName", fullName.value)
        localStorage.setItem("email", email.value)
        localStorage.setItem("username", username.value)
        localStorage.setItem("role", role.value)
    } catch (error) {
        console.error("Load profile failed:", error)
        window.$toast?.("Failed to load profile.", "error")
    }
}

const saveChanges = async () => {
    try {
        loadingProfile.value = true

        const payload = {
            fullName: fullName.value,
            email: email.value
        }

        const res = await updateMyProfile(payload)

        localStorage.setItem("fullName", res.data.fullName || fullName.value)
        localStorage.setItem("email", res.data.email || email.value)

        window.$toast?.("Profile updated successfully.", "success")
    } catch (error) {
        console.error("Update profile failed:", error)
        const message = error.response?.data?.message || "Failed to update profile."
        window.$toast?.(message, "error")
    } finally {
        loadingProfile.value = false
    }
}

const handleSendOtp = async () => {
    try {
        loadingOtp.value = true

        const payload = {
            currentPassword: currentPassword.value,
            newPassword: newPassword.value,
            confirmPassword: confirmPassword.value
        }

        const res = await sendChangePasswordOtp(payload)
        otpSent.value = true
        clearOtp()

        setTimeout(() => {
            otpRefs.value[0]?.focus()
        }, 120)

        window.$toast?.(res.data.message || "OTP sent successfully.", "success")
    } catch (error) {
        console.error("Send OTP failed:", error)
        const message = error.response?.data?.message || "Failed to send OTP."
        window.$toast?.(message, "error")
    } finally {
        loadingOtp.value = false
    }
}

const handleConfirmPasswordChange = async () => {
    try {
        loadingConfirm.value = true

        const payload = {
            currentPassword: currentPassword.value,
            newPassword: newPassword.value,
            confirmPassword: confirmPassword.value,
            otpCode: getOtpCode()
        }

        const res = await confirmChangePassword(payload)

        currentPassword.value = ""
        newPassword.value = ""
        confirmPassword.value = ""
        clearOtp()
        otpSent.value = false

        window.$toast?.(res.data.message || "Password changed successfully.", "success")
    } catch (error) {
        console.error("Confirm password change failed:", error)
        const message = error.response?.data?.message || "Failed to change password."
        window.$toast?.(message, "error")
    } finally {
        loadingConfirm.value = false
    }
}

onMounted(() => {
    loadProfile()
})
</script>

<style scoped>
.profile-page {
    min-height: calc(100vh - 120px);
    padding: 24px;
    background: linear-gradient(135deg, #0f7f96, #0d6f86);
    border-radius: 24px;
}

.profile-card {
    width: 100%;
    max-width: 1100px;
    margin: 0 auto;
    background: #ffffff;
    border-radius: 24px;
    box-shadow: 0 10px 24px rgba(0, 0, 0, 0.08);
    padding: 30px;
}

.profile-header {
    display: flex;
    align-items: center;
    gap: 22px;
    padding-bottom: 24px;
    border-bottom: 1px solid #e5e7eb;
}

.avatar-wrap {
    position: relative;
    width: 96px;
    height: 96px;
    flex-shrink: 0;
}

.avatar {
    width: 96px;
    height: 96px;
    border-radius: 50%;
    background: linear-gradient(135deg, #4f46e5, #2563eb);
    color: white;
    font-size: 40px;
    font-weight: 800;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 16px 34px rgba(37, 99, 235, 0.25);
}

.status-dot {
    position: absolute;
    right: 4px;
    bottom: 4px;
    width: 16px;
    height: 16px;
    border-radius: 50%;
    background: #cbd5e1;
    border: 3px solid white;
}

.status-dot.active {
    background: #22c55e;
}

.profile-title h2 {
    margin: 0;
    font-size: 30px;
    font-weight: 800;
    color: #1f2937;
}

.profile-title p {
    margin: 8px 0 12px;
    color: #64748b;
    font-size: 15px;
}

.role-chip {
    display: inline-flex;
    align-items: center;
    padding: 8px 14px;
    border-radius: 999px;
    background: #eef4ff;
    color: #2563eb;
    font-size: 13px;
    font-weight: 800;
}

.profile-body {
    padding-top: 26px;
    display: grid;
    gap: 22px;
}

.section-card {
    background: #ffffff;
    border: 1px solid #edf2f7;
    border-radius: 20px;
    padding: 22px;
    box-shadow: 0 4px 12px rgba(15, 23, 42, 0.04);
}

.section-head {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 20px;
}

.section-head.column {
    flex-direction: column;
    margin-bottom: 18px;
}

.section-title {
    font-size: 22px;
    font-weight: 800;
    color: #1e293b;
}

.section-subtitle {
    margin: 8px 0 0;
    color: #64748b;
    font-size: 14px;
}

.info-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 18px 22px;
}

.password-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 18px;
}

.info-item label {
    display: block;
    margin-bottom: 10px;
    font-size: 14px;
    font-weight: 800;
    color: #1e293b;
}

.info-item input {
    width: 100%;
    height: 56px;
    border: 1px solid #dbe3f0;
    border-radius: 16px;
    outline: none;
    padding: 0 16px;
    font-size: 15px;
    color: #334155;
    background: #f8fbff;
    transition: background-color 0.15s ease, color 0.15s ease, border-color 0.15s ease;
}

.info-item input:focus {
    border-color: #3b82f6;
    background: #ffffff;
    box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.12);
}

.info-item input:disabled {
    color: #94a3b8;
    background: #f8fafc;
    cursor: not-allowed;
}

.password-field {
    position: relative;
}

.password-field input {
    padding-right: 54px;
}

.toggle-password {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    width: 34px;
    height: 34px;
    border: none;
    border-radius: 10px;
    background: transparent;
    color: #64748b;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
}

.toggle-password:hover {
    background: #eef4ff;
    color: #2563eb;
}

.toggle-password svg {
    width: 20px;
    height: 20px;
}

.password-rules {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
    margin-top: 18px;
}

.password-rules div {
    min-height: 44px;
    border-radius: 14px;
    background: #f8fafc;
    color: #64748b;
    font-size: 14px;
    font-weight: 700;
    display: flex;
    align-items: center;
    padding: 0 16px;
    border: 1px solid #e5e7eb;
}

.password-rules div.ok {
    background: #ecfdf5;
    border-color: #a7f3d0;
    color: #047857;
}

.otp-actions,
.otp-confirm-row {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
}

.otp-panel {
    margin-top: 22px;
    padding: 22px;
    border-radius: 22px;
    background: linear-gradient(135deg, #f8fbff, #f1f7ff);
    border: 1px solid #dbeafe;
}

.otp-title {
    font-size: 18px;
    font-weight: 800;
    color: #1e293b;
}

.otp-subtitle {
    margin: 8px 0 18px;
    color: #64748b;
    font-size: 14px;
}

.otp-boxes {
    display: flex;
    gap: 12px;
    justify-content: center;
}

.otp-input {
    width: 58px;
    height: 64px;
    border: 1px solid #dbe3f0;
    border-radius: 18px;
    text-align: center;
    font-size: 24px;
    font-weight: 800;
    color: #1e293b;
    background: white;
    outline: none;
    transition: background-color 0.15s ease, color 0.15s ease, border-color 0.15s ease;
}

.otp-input:focus {
    border-color: #3b82f6;
    box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.12);
}

.save-btn,
.otp-btn,
.confirm-btn {
    min-width: 190px;
    height: 56px;
    border: none;
    border-radius: 16px;
    color: #ffffff;
    font-size: 15px;
    font-weight: 800;
    cursor: pointer;
    transition: background-color 0.15s ease, color 0.15s ease, border-color 0.15s ease;
}

.save-btn {
    background: linear-gradient(135deg, #3b82f6, #2563eb);
    box-shadow: 0 14px 28px rgba(37, 99, 235, 0.22);
}

.otp-btn {
    background: linear-gradient(135deg, #8b5cf6, #7c3aed);
    box-shadow: 0 14px 28px rgba(124, 58, 237, 0.24);
}

.confirm-btn {
    background: linear-gradient(135deg, #10b981, #059669);
    box-shadow: 0 14px 28px rgba(5, 150, 105, 0.22);
}

.save-btn:hover:not(:disabled),
.otp-btn:hover:not(:disabled),
.confirm-btn:hover:not(:disabled) {
    opacity: 0.95;
}

.save-btn:disabled,
.otp-btn:disabled,
.confirm-btn:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.fade-slide-enter-active,
.fade-slide-leave-active {
    transition: opacity 0.12s ease;
}

.fade-slide-enter-from,
.fade-slide-leave-to {
    opacity: 0;
}

@media (max-width: 992px) {

    .info-grid,
    .password-grid,
    .password-rules {
        grid-template-columns: 1fr;
    }
}

@media (max-width: 768px) {
    .profile-page {
        padding: 14px;
        border-radius: 18px;
    }

    .profile-card {
        padding: 18px;
        border-radius: 22px;
    }

    .profile-header {
        align-items: flex-start;
        gap: 16px;
    }

    .avatar-wrap,
    .avatar {
        width: 74px;
        height: 74px;
    }

    .avatar {
        font-size: 30px;
    }

    .profile-title h2 {
        font-size: 24px;
    }

    .section-card {
        padding: 18px;
        border-radius: 18px;
    }

    .section-head {
        flex-direction: column;
    }

    .save-btn,
    .otp-btn,
    .confirm-btn {
        width: 100%;
        min-width: unset;
    }

    .otp-actions,
    .otp-confirm-row {
        justify-content: stretch;
    }

    .otp-boxes {
        gap: 8px;
    }

    .otp-input {
        width: 44px;
        height: 54px;
        font-size: 20px;
        border-radius: 14px;
    }
}
</style>