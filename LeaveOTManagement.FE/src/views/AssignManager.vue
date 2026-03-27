<template>
    <div class="assign-page">
        <div class="assign-card">
            <div class="header">
                <div>
                    <h2>Assign Manager</h2>
                </div>

                <button class="back-btn" @click="goBack">
                    ← Back
                </button>
            </div>

            <div class="form-grid">
                <div class="form-group full-width">
                    <label>Employee</label>
                    <select v-model="userId">
                        <option value="">Select Employee</option>
                        <option v-for="u in employeeOptions" :key="u.id" :value="u.id">
                            {{ u.fullName }}{{ u.employeeCode ? ` (${u.employeeCode})` : "" }}
                        </option>
                    </select>
                </div>

                <div class="form-group full-width">
                    <label>Manager</label>
                    <select v-model="managerId">
                        <option value="">Select Manager</option>
                        <option v-for="m in filteredManagers" :key="m.id" :value="m.id">
                            {{ m.fullName }}{{ m.employeeCode ? ` (${m.employeeCode})` : "" }}
                        </option>
                    </select>
                </div>
            </div>

            <div v-if="selectedEmployee" class="preview-card">
                <div class="preview-row">
                    <span class="preview-label">Employee</span>
                    <span class="preview-value">{{ selectedEmployee.fullName }}</span>
                </div>

                <div class="preview-row">
                    <span class="preview-label">Current Manager</span>
                    <span class="preview-value">
                        {{ selectedEmployee.manager || selectedEmployee.managerName || "Not assigned" }}
                    </span>
                </div>

                <div class="preview-row" v-if="selectedManager">
                    <span class="preview-label">New Manager</span>
                    <span class="preview-value">{{ selectedManager.fullName }}</span>
                </div>
            </div>

            <div class="footer">
                <button class="secondary-btn" @click="resetForm">
                    Reset
                </button>

                <button class="primary-btn" @click="assignManager" :disabled="loading">
                    {{ loading ? "Assigning..." : "Assign Manager" }}
                </button>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()

const users = ref([])
const managers = ref([])

const userId = ref("")
const managerId = ref("")
const loading = ref(false)

const normalizeText = (value) => String(value || "").trim().toLowerCase()

const getRoleName = (user) => user.role || user.roleName || ""

const employeeOptions = computed(() => {
    return users.value.filter((u) => normalizeText(getRoleName(u)) !== "admin")
})

const filteredManagers = computed(() => {
    return managers.value.filter((m) => String(m.id) !== String(userId.value))
})

const selectedEmployee = computed(() => {
    return users.value.find((u) => String(u.id) === String(userId.value)) || null
})

const selectedManager = computed(() => {
    return managers.value.find((m) => String(m.id) === String(managerId.value)) || null
})

const loadData = async () => {
    try {
        const [userRes, managerRes] = await Promise.all([
            api.get("/users"),
            api.get("/users/managers")
        ])

        users.value = Array.isArray(userRes.data) ? userRes.data : []
        managers.value = Array.isArray(managerRes.data) ? managerRes.data : []
    } catch (err) {
        window.$toast?.("Failed to load data", "error")
    }
}

onMounted(loadData)

const resetForm = () => {
    userId.value = ""
    managerId.value = ""
}

const assignManager = async () => {
    if (!userId.value) {
        window.$toast?.("Please select an employee", "warning")
        return
    }

    if (!managerId.value) {
        window.$toast?.("Please select a manager", "warning")
        return
    }

    if (String(userId.value) === String(managerId.value)) {
        window.$toast?.("Employee cannot be their own manager", "warning")
        return
    }

    loading.value = true

    try {
        await api.put(`/users/assign-manager?userId=${userId.value}&managerId=${managerId.value}`)

        window.$toast?.("Manager assigned successfully", "success")
        resetForm()
        router.push("/user-management")
    } catch (err) {
        window.$toast?.("Error assigning manager", "error")
    } finally {
        loading.value = false
    }
}

const goBack = () => {
    router.push("/user-management")
}
</script>

<style scoped>
.assign-page {
    background: #f4f7fb;
    min-height: 100%;
    padding: 20px;
}

.assign-card {
    max-width: 800px;
    margin: auto;
    background: white;
    padding: 30px;
    border-radius: 18px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}

.header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 24px;
    flex-wrap: wrap;
}

.header h2 {
    margin: 0;
    color: #2b3674;
}

.header p {
    margin: 6px 0 0;
    color: #707eae;
}

.back-btn {
    padding: 10px 16px;
    border: 1px solid #d6dce8;
    background: white;
    border-radius: 8px;
    cursor: pointer;
    font-weight: 600;
    transition: 0.2s;
}

.back-btn:hover {
    background: #f8f9fc;
}

.form-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 16px;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.form-group label {
    font-weight: 600;
    color: #2b3674;
    font-size: 14px;
}

.full-width {
    grid-column: 1 / -1;
}

select {
    width: 100%;
    padding: 12px 14px;
    border: 1px solid #d6dce8;
    border-radius: 8px;
    font-size: 14px;
    background: white;
    outline: none;
}

select:focus {
    border-color: #4318ff;
    box-shadow: 0 0 0 3px rgba(67, 24, 255, 0.08);
}

.preview-card {
    margin-top: 22px;
    background: #f8f9fc;
    border: 1px solid #e9edf5;
    border-radius: 14px;
    padding: 18px;
}

.preview-row {
    display: flex;
    justify-content: space-between;
    gap: 16px;
    padding: 10px 0;
    border-bottom: 1px solid #eef2f7;
}

.preview-row:last-child {
    border-bottom: none;
}

.preview-label {
    color: #707eae;
    font-weight: 600;
}

.preview-value {
    color: #2b3674;
    font-weight: 700;
    text-align: right;
}

.footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    margin-top: 26px;
    flex-wrap: wrap;
}

.primary-btn,
.secondary-btn {
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-weight: 600;
    padding: 10px 18px;
    transition: 0.2s;
}

.primary-btn {
    background: #4318ff;
    color: white;
}

.primary-btn:disabled {
    opacity: 0.65;
    cursor: not-allowed;
}

.secondary-btn {
    background: #eef2f7;
    color: #334155;
}

@media (max-width: 768px) {
    .assign-card {
        padding: 18px;
    }

    .form-grid {
        grid-template-columns: 1fr;
    }

    .header {
        flex-direction: column;
    }

    .footer {
        flex-direction: column-reverse;
    }

    .primary-btn,
    .secondary-btn,
    .back-btn {
        width: 100%;
    }

    .preview-row {
        flex-direction: column;
        align-items: flex-start;
    }

    .preview-value {
        text-align: left;
    }
}
</style>