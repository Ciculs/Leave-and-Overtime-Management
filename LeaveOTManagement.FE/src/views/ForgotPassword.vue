<template>
    <div class="forgot-page">
        <div class="forgot-card">
            <div class="forgot-header">
                <div class="brand-badge">LOM</div>
                <div class="forgot-title">
                    <h2>Forgot Password</h2>
                    <p>Reset your password securely with email OTP verification</p>
                </div>
            </div>

            <div class="step-indicator">
                <div class="step" :class="{ active: step >= 1 }">
                    <span>1</span>
                    <small>Account</small>
                </div>
                <div class="step-line" :class="{ active: step >= 2 }"></div>
                <div class="step" :class="{ active: step >= 2 }">
                    <span>2</span>
                    <small>OTP</small>
                </div>
                <div class="step-line" :class="{ active: step >= 3 }"></div>
                <div class="step" :class="{ active: step >= 3 }">
                    <span>3</span>
                    <small>Password</small>
                </div>
            </div>

            <div class="forgot-body">
                <div v-if="step === 1" class="section-block">
                    <div class="section-title">Find your account</div>
                    <p class="section-subtitle">
                        Enter your username or email to receive a verification OTP.
                    </p>

                    <div class="form-group">
                        <label>Username or Email</label>
                        <input v-model="identifier" type="text" placeholder="Enter username or email" />
                    </div>

                    <div class="action-row">
                        <button class="primary-btn" @click="handleSendOtp" :disabled="loadingSendOtp">
                            {{ loadingSendOtp ? "Sending OTP..." : "Send OTP" }}
                        </button>
                        <button class="ghost-btn" @click="goLogin">
                            Back to Login
                        </button>
                    </div>
                </div>

                <div v-if="step === 2" class="section-block">
                    <div class="section-title">Verify OTP</div>
                    <p class="section-subtitle">
                        We sent a 6-digit OTP to your email. Enter it below to continue.
                    </p>

                    <div class="otp-boxes">
                        <input v-for="(digit, index) in otpDigits" :key="index" :ref="el => setOtpRef(el, index)"
                            v-model="otpDigits[index]" type="text" maxlength="1" class="otp-input"
                            @input="handleOtpInput(index, $event)"
                            @keydown.backspace="handleOtpBackspace(index, $event)" />
                    </div>

                    <div class="helper-row">
                        <span class="otp-hint">{{ otpMessage }}</span>

                        <button type="button" class="link-btn" @click="handleResendOtp"
                            :disabled="countdown > 0 || loadingResendOtp">
                            {{
                                loadingResendOtp
                                    ? "Resending..."
                                    : countdown > 0
                                        ? `Resend in ${countdown}s`
                            : "Resend OTP"
                            }}
                        </button>
                    </div>

                    <div class="action-row">
                        <button class="primary-btn" @click="handleVerifyOtp" :disabled="loadingVerifyOtp">
                            {{ loadingVerifyOtp ? "Verifying..." : "Verify OTP" }}
                        </button>
                        <button class="ghost-btn" @click="step = 1">
                            Back
                        </button>
                    </div>
                </div>

                <div v-if="step === 3" class="section-block">
                    <div class="section-title">Set new password</div>
                    <p class="section-subtitle">
                        Create a new password for your account.
                    </p>

                    <div class="form-group">
                        <label>New Password</label>
                        <div class="password-field">
                            <input v-model="newPassword" :type="showNewPassword ? 'text' : 'password'"
                                placeholder="Enter new password" />
                            <button type="button" class="toggle-password" @click="showNewPassword = !showNewPassword">
                                <svg v-if="!showNewPassword" viewBox="0 0 24 24" fill="none">
                                    <path d="M2 12S5.5 5 12 5s10 7 10 7-3.5 7-10 7S2 12 2 12Z" stroke="currentColor"
                                        stroke-width="2" />
                                    <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" />
                                </svg>
                                <svg v-else viewBox="0 0 24 24" fill="none">
                                    <path d="M3 3L21 21" stroke="currentColor" stroke-width="2" />
                                    <path d="M10.5 10.7A3 3 0 0 0 13.3 13.5" stroke="currentColor" stroke-width="2" />
                                    <path d="M9.9 5.1A11 11 0 0 1 12 5c6.5 0 10 7 10 7a17.3 17.3 0 0 1-4.2 5.1"
                                        stroke="currentColor" stroke-width="2" />
                                    <path d="M6.2 6.2A17.7 17.7 0 0 0 2 12s3.5 7 10 7a10.7 10.7 0 0 0 3-.4"
                                        stroke="currentColor" stroke-width="2" />
                                </svg>
                            </button>
                        </div>
                    </div>

                    <div class="form-group">
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
                                    <path d="M10.5 10.7A3 3 0 0 0 13.3 13.5" stroke="currentColor" stroke-width="2" />
                                    <path d="M9.9 5.1A11 11 0 0 1 12 5c6.5 0 10 7 10 7a17.3 17.3 0 0 1-4.2 5.1"
                                        stroke="currentColor" stroke-width="2" />
                                    <path d="M6.2 6.2A17.7 17.7 0 0 0 2 12s3.5 7 10 7a10.7 10.7 0 0 0 3-.4"
                                        stroke="currentColor" stroke-width="2" />
                                </svg>
                            </button>
                        </div>
                    </div>

                    <div class="password-rules">
                        <div :class="{ ok: newPassword.length >= 6 }">At least 6 characters</div>
                        <div :class="{ ok: confirmPassword && newPassword === confirmPassword }">
                            Password confirmation matches
                        </div>
                    </div>

                    <div class="action-row">
                        <button class="primary-btn success-btn" @click="handleResetPassword" :disabled="loadingReset">
                            {{ loadingReset ? "Resetting..." : "Reset Password" }}
                        </button>
                        <button class="ghost-btn" @click="step = 2">
                            Back
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { onBeforeUnmount, ref } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()

const step = ref(1)
const identifier = ref("")
const otpDigits = ref(["", "", "", "", "", ""])
const otpRefs = ref([])
const otpMessage = ref("Didn't receive the code? You can resend after countdown ends.")
const countdown = ref(0)
let countdownTimer = null

const newPassword = ref("")
const confirmPassword = ref("")
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

const loadingSendOtp = ref(false)
const loadingResendOtp = ref(false)
const loadingVerifyOtp = ref(false)
const loadingReset = ref(false)

const tempToken = ref("")

const setOtpRef = (el, index) => {
    if (el) otpRefs.value[index] = el
}

const startCountdown = () => {
    countdown.value = 60
    clearInterval(countdownTimer)
    countdownTimer = setInterval(() => {
        if (countdown.value > 0) {
            countdown.value -= 1
        } else {
            clearInterval(countdownTimer)
        }
    }, 1000)
}

const getOtpCode = () => otpDigits.value.join("").trim()

const clearOtp = () => {
    otpDigits.value = ["", "", "", "", "", ""]
}

const goLogin = () => {
    router.push("/login")
}

const handleOtpInput = (index, event) => {
    const value = event.target.value.replace(/\D/g, "")
    otpDigits.value[index] = value

    if (value && index < 5) {
        otpRefs.value[index + 1]?.focus()
    }
}

const handleOtpBackspace = (index, event) => {
    if (!otpDigits.value[index] && index > 0) {
        otpRefs.value[index - 1]?.focus()
    } else if (event.target.value) {
        otpDigits.value[index] = ""
    }
}

const handleSendOtp = async () => {
    try {
        if (!identifier.value.trim()) {
            window.$toast?.("Please enter username or email.", "warning")
            return
        }

        loadingSendOtp.value = true

        const res = await api.post("/Auth/forgot-password/send-otp", {
            identifier: identifier.value.trim()
        })

        otpMessage.value = res.data.message || "OTP has been sent successfully."
        step.value = 2
        clearOtp()
        startCountdown()

        setTimeout(() => {
            otpRefs.value[0]?.focus()
        }, 100)

        window.$toast?.("OTP sent successfully.", "success")
    } catch (error) {
        console.error("Send forgot password OTP failed:", error)
        const message = error.response?.data?.message || "Failed to send OTP."
        window.$toast?.(message, "error")
    } finally {
        loadingSendOtp.value = false
    }
}

const handleResendOtp = async () => {
    try {
        if (!identifier.value.trim()) {
            window.$toast?.("Please enter username or email.", "warning")
            return
        }

        loadingResendOtp.value = true

        const res = await api.post("/Auth/forgot-password/send-otp", {
            identifier: identifier.value.trim()
        })

        otpMessage.value = res.data.message || "OTP resent successfully."
        clearOtp()
        startCountdown()

        setTimeout(() => {
            otpRefs.value[0]?.focus()
        }, 100)

        window.$toast?.("OTP resent successfully.", "success")
    } catch (error) {
        console.error("Resend OTP failed:", error)
        const message = error.response?.data?.message || "Failed to resend OTP."
        window.$toast?.(message, "error")
    } finally {
        loadingResendOtp.value = false
    }
}

const handleVerifyOtp = async () => {
    try {
        const otpCode = getOtpCode()

        if (otpCode.length !== 6) {
            window.$toast?.("Please enter full 6-digit OTP.", "warning")
            return
        }

        loadingVerifyOtp.value = true

        const res = await api.post("/Auth/forgot-password/verify-otp", {
            identifier: identifier.value.trim(),
            otpCode
        })

        tempToken.value = res.data.resetToken || ""
        step.value = 3
        window.$toast?.(res.data.message || "OTP verified successfully.", "success")
    } catch (error) {
        console.error("Verify OTP failed:", error)
        const message = error.response?.data?.message || "OTP verification failed."
        window.$toast?.(message, "error")
    } finally {
        loadingVerifyOtp.value = false
    }
}

const handleResetPassword = async () => {
    try {
        if (!newPassword.value.trim()) {
            window.$toast?.("Please enter new password.", "warning")
            return
        }

        if (newPassword.value.length < 6) {
            window.$toast?.("New password must be at least 6 characters.", "warning")
            return
        }

        if (newPassword.value !== confirmPassword.value) {
            window.$toast?.("Confirm password does not match.", "warning")
            return
        }

        loadingReset.value = true

        const res = await api.post("/Auth/forgot-password/reset", {
            identifier: identifier.value.trim(),
            resetToken: tempToken.value,
            newPassword: newPassword.value,
            confirmPassword: confirmPassword.value
        })

        window.$toast?.(res.data.message || "Password reset successfully.", "success")

        setTimeout(() => {
            router.push("/login")
        }, 900)
    } catch (error) {
        console.error("Reset password failed:", error)
        const message = error.response?.data?.message || "Failed to reset password."
        window.$toast?.(message, "error")
    } finally {
        loadingReset.value = false
    }
}

onBeforeUnmount(() => {
    clearInterval(countdownTimer)
})
</script>

<style scoped>
.forgot-page {
    min-height: 100vh;
    padding: 28px;
    display: flex;
    justify-content: center;
    align-items: center;
    background:
        radial-gradient(circle at top left, rgba(59, 130, 246, 0.18), transparent 28%),
        radial-gradient(circle at bottom right, rgba(14, 165, 233, 0.16), transparent 32%),
        linear-gradient(135deg, #0f7f96, #0d6f86);
}

.forgot-card {
    width: 100%;
    max-width: 760px;
    background: rgba(255, 255, 255, 0.96);
    border-radius: 30px;
    box-shadow: 0 24px 60px rgba(0, 0, 0, 0.16);
    padding: 38px 42px;
    backdrop-filter: blur(8px);
}

.forgot-header {
    display: flex;
    align-items: center;
    gap: 18px;
    padding-bottom: 24px;
    border-bottom: 1px solid #e5e7eb;
}

.brand-badge {
    width: 64px;
    height: 64px;
    border-radius: 20px;
    background: linear-gradient(135deg, #4f46e5, #2563eb);
    color: white;
    font-weight: 800;
    font-size: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 14px 28px rgba(37, 99, 235, 0.24);
    flex-shrink: 0;
}

.forgot-title h2 {
    margin: 0;
    font-size: 30px;
    font-weight: 800;
    color: #1f2937;
}

.forgot-title p {
    margin: 8px 0 0;
    color: #64748b;
    font-size: 15px;
}

.step-indicator {
    margin-top: 26px;
    display: flex;
    align-items: center;
    gap: 10px;
}

.step {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    min-width: 70px;
}

.step span {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    background: #e5e7eb;
    color: #64748b;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 800;
    transition: 0.25s ease;
}

.step small {
    color: #64748b;
    font-size: 12px;
    font-weight: 700;
}

.step.active span {
    background: linear-gradient(135deg, #3b82f6, #2563eb);
    color: #ffffff;
}

.step.active small {
    color: #1e293b;
}

.step-line {
    height: 3px;
    flex: 1;
    border-radius: 999px;
    background: #e5e7eb;
}

.step-line.active {
    background: linear-gradient(90deg, #3b82f6, #2563eb);
}

.forgot-body {
    padding-top: 28px;
}

.section-block {
    animation: fadeUp 0.25s ease;
}

.section-title {
    font-size: 22px;
    font-weight: 800;
    color: #1e293b;
    margin-bottom: 8px;
}

.section-subtitle {
    margin: 0 0 22px;
    color: #64748b;
    font-size: 14px;
}

.form-group {
    margin-bottom: 20px;
}

.form-group label {
    display: block;
    margin-bottom: 10px;
    font-size: 15px;
    font-weight: 700;
    color: #1e293b;
}

.form-group input {
    width: 100%;
    height: 58px;
    border: 1px solid #dbe3f0;
    border-radius: 16px;
    outline: none;
    padding: 0 18px;
    font-size: 15px;
    color: #334155;
    background: #f8fbff;
    transition: 0.25s ease;
}

.form-group input:focus {
    border-color: #3b82f6;
    background: #ffffff;
    box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.12);
}

.password-field {
    position: relative;
}

.password-field input {
    padding-right: 54px;
}

.toggle-password {
    position: absolute;
    right: 14px;
    top: 50%;
    transform: translateY(-50%);
    width: 34px;
    height: 34px;
    border: none;
    border-radius: 10px;
    background: transparent;
    color: #64748b;
    cursor: pointer;
    transition: 0.2s ease;
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

.otp-boxes {
    display: flex;
    gap: 12px;
    justify-content: center;
    margin-bottom: 16px;
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
    background: #f8fbff;
    outline: none;
    transition: 0.25s ease;
}

.otp-input:focus {
    border-color: #3b82f6;
    background: white;
    box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.12);
}

.helper-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 16px;
    margin-bottom: 18px;
}

.otp-hint {
    color: #64748b;
    font-size: 13px;
}

.link-btn {
    border: none;
    background: transparent;
    color: #2563eb;
    font-weight: 700;
    cursor: pointer;
    padding: 0;
}

.link-btn:disabled {
    color: #94a3b8;
    cursor: not-allowed;
}

.password-rules {
    display: grid;
    gap: 10px;
    margin-bottom: 10px;
}

.password-rules div {
    min-height: 42px;
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

.action-row {
    display: flex;
    gap: 14px;
    margin-top: 24px;
}

.primary-btn,
.ghost-btn {
    height: 56px;
    padding: 0 24px;
    border-radius: 16px;
    border: none;
    font-size: 15px;
    font-weight: 800;
    cursor: pointer;
    transition: 0.25s ease;
}

.primary-btn {
    background: linear-gradient(135deg, #3b82f6, #2563eb);
    color: white;
    box-shadow: 0 14px 28px rgba(37, 99, 235, 0.22);
}

.primary-btn:hover:not(:disabled) {
    transform: translateY(-2px);
}

.success-btn {
    background: linear-gradient(135deg, #10b981, #059669);
    box-shadow: 0 14px 28px rgba(5, 150, 105, 0.22);
}

.ghost-btn {
    background: #eff6ff;
    color: #1d4ed8;
}

.ghost-btn:hover {
    background: #dbeafe;
}

.primary-btn:disabled,
.ghost-btn:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

@keyframes fadeUp {
    from {
        opacity: 0;
        transform: translateY(8px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@media (max-width: 768px) {
    .forgot-page {
        padding: 16px;
    }

    .forgot-card {
        padding: 24px 18px;
        border-radius: 22px;
    }

    .forgot-header {
        align-items: flex-start;
    }

    .brand-badge {
        width: 54px;
        height: 54px;
        border-radius: 16px;
        font-size: 18px;
    }

    .forgot-title h2 {
        font-size: 24px;
    }

    .step-indicator {
        gap: 8px;
    }

    .step {
        min-width: 56px;
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

    .helper-row {
        flex-direction: column;
        align-items: flex-start;
    }

    .action-row {
        flex-direction: column;
    }

    .primary-btn,
    .ghost-btn {
        width: 100%;
    }
}
</style>