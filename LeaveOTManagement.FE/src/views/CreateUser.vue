<template>
    <div class="create-user-page">
        <div class="create-user-card">
            <div class="header">
                <div>
                    <h2>Create User</h2>
                    <p>Add a new employee account to the system.</p>
                </div>

                <button class="back-btn" @click="goBack">
                    ← Back
                </button>
            </div>

            <div class="form-grid">
                <div class="form-group">
                    <label>Full Name</label>
                    <input v-model="fullName" placeholder="Enter full name" />
                </div>

                <div class="form-group">
                    <label>Email</label>
                    <input v-model="email" placeholder="Enter email" />
                </div>

                <div class="form-group">
                    <label>Username</label>
                    <input v-model="username" placeholder="Enter username" />
                </div>

                <div class="form-group">
                    <label>Password</label>
                    <input v-model="password" type="password" placeholder="Enter password" />
                </div>

                <div class="form-group">
                    <label>Department</label>
                    <select v-model="departmentId">
                        <option value="">Select Department</option>
                        <option v-for="d in departments" :key="d.id" :value="d.id">
                            {{ d.name }}
                        </option>
                    </select>
                </div>

                <div class="form-group">
                    <label>Role</label>
                    <select v-model="roleId">
                        <option value="">Select Role</option>
                        <option v-for="r in roles" :key="r.id" :value="r.id">
                            {{ r.name }}
                        </option>
                    </select>
                </div>

                <div class="form-group full-width">
                    <label>Manager</label>
                    <select v-model="managerId">
                        <option value="">No Manager</option>
                        <option v-for="m in managers" :key="m.id" :value="m.id">
                            {{ m.fullName }}
                        </option>
                    </select>
                </div>
            </div>

            <div class="footer">
                <button class="secondary-btn" @click="goBack">
                    Cancel
                </button>

                <button class="primary-btn" @click="createUser" :disabled="loading">
                    {{ loading ? "Creating..." : "Create User" }}
                </button>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()

const fullName = ref("")
const email = ref("")
const username = ref("")
const password = ref("")

const departmentId = ref("")
const roleId = ref("")
const managerId = ref("")

const departments = ref([])
const roles = ref([])
const managers = ref([])

const loading = ref(false)

const loadData = async () => {
    try {
        const [depRes, roleRes, managerRes] = await Promise.all([
            api.get("/users/departments"),
            api.get("/users/roles"),
            api.get("/users/managers")
        ])

        departments.value = depRes.data || []
        roles.value = roleRes.data || []
        managers.value = managerRes.data || []
    } catch (err) {
        window.$toast?.("Failed to load data", "error")
    }
}

onMounted(loadData)

// ================= VALIDATE =================
const validateForm = () => {
    if (!fullName.value.trim()) {
        window.$toast?.("Full name is required", "warning")
        return false
    }

    if (!email.value.trim()) {
        window.$toast?.("Email is required", "warning")
        return false
    }

    if (!username.value.trim()) {
        window.$toast?.("Username is required", "warning")
        return false
    }

    if (!password.value.trim()) {
        window.$toast?.("Password is required", "warning")
        return false
    }

    if (!departmentId.value) {
        window.$toast?.("Please select department", "warning")
        return false
    }

    if (!roleId.value) {
        window.$toast?.("Please select role", "warning")
        return false
    }

    return true
}

// ================= CREATE =================
const createUser = async () => {
    if (!validateForm()) return

    loading.value = true

    try {
        const res = await api.post("/users", {
            fullName: fullName.value.trim(),
            email: email.value.trim(),
            username: username.value.trim(),
            password: password.value,
            departmentId: Number(departmentId.value),
            roleId: Number(roleId.value),
            managerId: managerId.value ? Number(managerId.value) : null
        })

        window.$toast?.("User created successfully", "success")

        router.push("/user-management")
    } catch (err) {
        window.$toast?.("Create user failed", "error")
    } finally {
        loading.value = false
    }
}

const goBack = () => {
    router.push("/user-management")
}
</script>

<style scoped>
.create-user-page {
    background: #f4f7fb;
    min-height: 100%;
    padding: 20px;
}

.create-user-card {
    max-width: 800px;
    margin: auto;
    background: white;
    padding: 30px;
    border-radius: 18px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}

/* HEADER */
.header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 25px;
}

.header h2 {
    margin: 0;
    color: #2b3674;
}

.header p {
    margin: 5px 0 0;
    color: #707eae;
}

.back-btn {
    border: 1px solid #ddd;
    background: white;
    padding: 8px 14px;
    border-radius: 8px;
    cursor: pointer;
}

/* FORM */
.form-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.form-group label {
    font-weight: 600;
    color: #2b3674;
}

input,
select {
    padding: 10px;
    border-radius: 8px;
    border: 1px solid #ccc;
}

.full-width {
    grid-column: span 2;
}

/* FOOTER */
.footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    margin-top: 25px;
}

.primary-btn {
    background: #4318ff;
    color: white;
    padding: 10px 18px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
}

.secondary-btn {
    background: #eee;
    padding: 10px 18px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
}

.primary-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

/* MOBILE */
@media (max-width: 768px) {
    .form-grid {
        grid-template-columns: 1fr;
    }

    .full-width {
        grid-column: span 1;
    }
}
</style>