<template>
    <div class="user-management-page">
        <div class="page-card">
            <div class="page-header">
                <div>
                    <h1>User Management</h1>
                </div>

                <div class="header-actions">
                    <button class="create-btn" @click="goCreateUser">
                        + Create User
                    </button>

                    <button class="assign-btn" @click="goAssignManager">
                        Assign Manager
                    </button>
                </div>
            </div>

            <!-- SEARCH + FILTER -->
            <div class="toolbar">
                <div class="search-box">
                    <input v-model="search" type="text"
                        placeholder="Search by code, name, email, department, manager..." />
                </div>

                <div class="filter-box">
                    <select v-model="roleFilter">
                        <option value="">All Roles</option>
                        <option v-for="r in roleOptions" :key="r" :value="r">
                            {{ r }}
                        </option>
                    </select>
                </div>
            </div>

            <!-- TABLE -->
            <div class="table-container">
                <table>
                    <thead>
                        <tr>
                            <th>Code</th>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Department</th>
                            <th>Role</th>
                            <th>Manager</th>
                            <th>Status</th>
                            <th class="action-col">Action</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-if="loading">
                            <td colspan="8" class="empty-cell">Loading users...</td>
                        </tr>

                        <tr v-else-if="paginatedUsers.length === 0">
                            <td colspan="8" class="empty-cell">No users found</td>
                        </tr>

                        <tr v-for="u in paginatedUsers" :key="u.id">
                            <td>{{ u.employeeCode || "-" }}</td>
                            <td>{{ u.fullName || "-" }}</td>
                            <td>{{ u.email || "-" }}</td>
                            <td>{{ getDepartmentName(u) }}</td>
                            <td>{{ getRoleName(u) }}</td>
                            <td>{{ getManagerName(u) }}</td>

                            <td>
                                <span class="status-badge" :class="normalizeActive(u) ? 'active' : 'inactive'">
                                    {{ normalizeActive(u) ? "Active" : "Inactive" }}
                                </span>
                            </td>

                            <td class="action-cell">
                                <button class="edit-btn" @click="openEditModal(u)">
                                    Edit
                                </button>

                                <button v-if="normalizeActive(u)" class="danger-btn"
                                    @click="openConfirmModal(u, 'deactivate')">
                                    Deactivate
                                </button>

                                <button v-else class="success-btn" @click="openConfirmModal(u, 'activate')">
                                    Activate
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- FOOTER -->
            <div class="table-footer" v-if="!loading && filteredUsers.length > 0">
                <div class="footer-left">
                    Showing
                    <strong>{{ startItem }}</strong>
                    -
                    <strong>{{ endItem }}</strong>
                    of
                    <strong>{{ filteredUsers.length }}</strong>
                    users
                </div>

                <div class="pagination">
                    <button class="page-btn" @click="goToPage(currentPage - 1)" :disabled="currentPage === 1">
                        Prev
                    </button>

                    <button v-for="page in visiblePages" :key="page" class="page-btn"
                        :class="{ active: currentPage === page }" @click="goToPage(page)">
                        {{ page }}
                    </button>

                    <button class="page-btn" @click="goToPage(currentPage + 1)" :disabled="currentPage === totalPages">
                        Next
                    </button>
                </div>
            </div>
        </div>

        <!-- EDIT MODAL -->
        <div v-if="showEditModal" class="modal-overlay" @click.self="closeEditModal">
            <div class="modal-card">
                <div class="modal-header">
                    <div>
                        <h3>Edit User</h3>
                        <p>Update account information and assignment.</p>
                    </div>

                    <button class="icon-close" @click="closeEditModal">✕</button>
                </div>

                <div class="modal-body">
                    <div class="form-grid">
                        <div class="form-group">
                            <label>Full Name</label>
                            <input v-model="editForm.fullName" type="text" />
                        </div>

                        <div class="form-group">
                            <label>Email</label>
                            <input v-model="editForm.email" type="email" />
                        </div>

                        <div class="form-group">
                            <label>Department</label>
                            <select v-model="editForm.departmentId">
                                <option value="">Select Department</option>
                                <option v-for="d in departments" :key="d.id" :value="String(d.id)">
                                    {{ d.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Role</label>
                            <select v-model="editForm.roleId">
                                <option value="">Select Role</option>
                                <option v-for="r in roles" :key="r.id" :value="String(r.id)">
                                    {{ r.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group full-width">
                            <label>Manager</label>
                            <select v-model="editForm.managerId">
                                <option value="">No Manager</option>
                                <option v-for="m in managers" :key="m.id" :value="String(m.id)">
                                    {{ m.fullName }}
                                </option>
                            </select>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button class="secondary-btn" @click="closeEditModal">
                        Cancel
                    </button>

                    <button class="primary-btn" @click="saveUser" :disabled="saving">
                        {{ saving ? "Saving..." : "Save Changes" }}
                    </button>
                </div>
            </div>
        </div>

        <!-- CONFIRM MODAL -->
        <div v-if="showConfirmModal" class="modal-overlay" @click.self="closeConfirmModal">
            <div class="confirm-card">
                <h3>
                    {{ confirmAction === "deactivate" ? "Deactivate User" : "Activate User" }}
                </h3>

                <p class="confirm-text">
                    Are you sure you want to
                    <strong>
                        {{ confirmAction === "deactivate" ? "deactivate" : "activate" }}
                    </strong>
                    <strong>{{ selectedUser?.fullName }}</strong>?
                </p>

                <div class="modal-footer">
                    <button class="secondary-btn" @click="closeConfirmModal">
                        Cancel
                    </button>

                    <button class="primary-btn"
                        :class="confirmAction === 'deactivate' ? 'danger-solid' : 'success-solid'"
                        @click="toggleUserStatus" :disabled="statusLoading">
                        {{
                            statusLoading
                                ? "Processing..."
                                : confirmAction === "deactivate"
                                    ? "Deactivate"
                        : "Activate"
                        }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()

const loading = ref(false)
const saving = ref(false)
const statusLoading = ref(false)

const allUsers = ref([])
const departments = ref([])
const roles = ref([])
const managers = ref([])

const search = ref("")
const roleFilter = ref("")
const currentPage = ref(1)
const pageSize = 5

const showEditModal = ref(false)
const showConfirmModal = ref(false)
const selectedUser = ref(null)
const confirmAction = ref("deactivate")

const editForm = ref({
    id: null,
    fullName: "",
    email: "",
    departmentId: "",
    roleId: "",
    managerId: ""
})

const normalizeText = (value) => {
    return String(value || "")
        .toLowerCase()
        .trim()
}

const normalizeActive = (user) => {
    return !!(user?.isActive ?? user?.active ?? false)
}

const getDepartmentName = (user) => {
    return (
        user.department ||
        user.departmentName ||
        departments.value.find(d => String(d.id) === String(user.departmentId))?.name ||
        "-"
    )
}

const getRoleName = (user) => {
    return (
        user.role ||
        user.roleName ||
        roles.value.find(r => String(r.id) === String(user.roleId))?.name ||
        "-"
    )
}

const getManagerName = (user) => {
    return (
        user.manager ||
        user.managerName ||
        managers.value.find(m => String(m.id) === String(user.managerId))?.fullName ||
        "-"
    )
}

const roleOptions = computed(() => {
    const set = new Set()

    allUsers.value.forEach(u => {
        const roleName = getRoleName(u)
        if (roleName && roleName !== "-") {
            set.add(roleName)
        }
    })

    roles.value.forEach(r => {
        if (r.name) set.add(r.name)
    })

    return Array.from(set)
})

const filteredUsers = computed(() => {
    let result = [...allUsers.value]

    const keyword = normalizeText(search.value)
    const selectedRole = normalizeText(roleFilter.value)

    if (keyword) {
        result = result.filter(u => {
            const haystack = [
                u.employeeCode,
                u.fullName,
                u.email,
                getDepartmentName(u),
                getRoleName(u),
                getManagerName(u)
            ]
                .map(normalizeText)
                .join(" ")

            return haystack.includes(keyword)
        })
    }

    if (selectedRole) {
        result = result.filter(u => normalizeText(getRoleName(u)) === selectedRole)
    }

    return result
})

const totalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredUsers.value.length / pageSize))
})

const paginatedUsers = computed(() => {
    const start = (currentPage.value - 1) * pageSize
    const end = start + pageSize
    return filteredUsers.value.slice(start, end)
})

const startItem = computed(() => {
    if (filteredUsers.value.length === 0) return 0
    return (currentPage.value - 1) * pageSize + 1
})

const endItem = computed(() => {
    return Math.min(currentPage.value * pageSize, filteredUsers.value.length)
})

const visiblePages = computed(() => {
    const total = totalPages.value
    const current = currentPage.value
    const pages = []

    let start = Math.max(1, current - 2)
    let end = Math.min(total, current + 2)

    if (current <= 3) {
        end = Math.min(total, 5)
    }

    if (current >= total - 2) {
        start = Math.max(1, total - 4)
    }

    for (let i = start; i <= end; i++) {
        pages.push(i)
    }

    return pages
})

watch([search, roleFilter], () => {
    currentPage.value = 1
})

watch(totalPages, (newValue) => {
    if (currentPage.value > newValue) {
        currentPage.value = newValue
    }
})

const goToPage = (page) => {
    if (page < 1 || page > totalPages.value) return
    currentPage.value = page
}

const loadLookupData = async () => {
    const [depRes, roleRes, managerRes] = await Promise.all([
        api.get("/users/departments"),
        api.get("/users/roles"),
        api.get("/users/managers")
    ])

    departments.value = depRes.data || []
    roles.value = roleRes.data || []
    managers.value = managerRes.data || []
}

const loadUsers = async () => {
    loading.value = true

    try {
        const res = await api.get("/Users")
        allUsers.value = Array.isArray(res.data) ? res.data : []
    } catch (error) {
        allUsers.value = []
        window.$toast?.("Failed to load users", "error")
    } finally {
        loading.value = false
    }
}

const openEditModal = (user) => {
    selectedUser.value = user

    const matchedDepartment =
        departments.value.find(d => normalizeText(d.name) === normalizeText(user.department || user.departmentName)) ||
        departments.value.find(d => String(d.id) === String(user.departmentId))

    const matchedRole =
        roles.value.find(r => normalizeText(r.name) === normalizeText(user.role || user.roleName)) ||
        roles.value.find(r => String(r.id) === String(user.roleId))

    const matchedManager =
        managers.value.find(m => normalizeText(m.fullName) === normalizeText(user.manager || user.managerName)) ||
        managers.value.find(m => String(m.id) === String(user.managerId))

    editForm.value = {
        id: user.id,
        fullName: user.fullName || "",
        email: user.email || "",
        departmentId: matchedDepartment ? String(matchedDepartment.id) : "",
        roleId: matchedRole ? String(matchedRole.id) : "",
        managerId: matchedManager ? String(matchedManager.id) : ""
    }

    showEditModal.value = true
}

const closeEditModal = () => {
    showEditModal.value = false
    selectedUser.value = null
}

const saveUser = async () => {
    if (!editForm.value.fullName.trim()) {
        window.$toast?.("Full name is required", "warning")
        return
    }

    if (!editForm.value.roleId || !editForm.value.departmentId) {
        window.$toast?.("Please select role and department", "warning")
        return
    }

    saving.value = true

    try {
        await api.put(`/Users/${editForm.value.id}`, {
            fullName: editForm.value.fullName.trim(),
            email: editForm.value.email?.trim() || "",
            roleId: Number(editForm.value.roleId),
            departmentId: Number(editForm.value.departmentId),
            managerId: editForm.value.managerId ? Number(editForm.value.managerId) : null
        })

        window.$toast?.("User updated successfully", "success")
        closeEditModal()
        await loadUsers()
    } catch (error) {
        window.$toast?.("Failed to update user", "error")
    } finally {
        saving.value = false
    }
}

const openConfirmModal = (user, action) => {
    selectedUser.value = user
    confirmAction.value = action
    showConfirmModal.value = true
}

const closeConfirmModal = () => {
    showConfirmModal.value = false
    selectedUser.value = null
}

const toggleUserStatus = async () => {
    if (!selectedUser.value?.id) return

    statusLoading.value = true

    try {
        if (confirmAction.value === "deactivate") {
            await api.put(`/Users/${selectedUser.value.id}/deactivate`)
            window.$toast?.("User deactivated successfully", "success")
        } else {
            await api.put(`/Users/${selectedUser.value.id}/activate`)
            window.$toast?.("User activated successfully", "success")
        }

        closeConfirmModal()
        await loadUsers()
    } catch (error) {
        window.$toast?.(
            confirmAction.value === "deactivate"
                ? "Failed to deactivate user"
                : "Failed to activate user",
            "error"
        )
    } finally {
        statusLoading.value = false
    }
}

const goCreateUser = () => {
    router.push("/create-user")
}

const goAssignManager = () => {
    router.push("/assign-manager")
}

onMounted(async () => {
    await Promise.all([loadLookupData(), loadUsers()])
})
</script>

<style scoped>
.user-management-page {
    background: #f4f7fb;
    min-height: 100%;
}

.page-card {
    background: white;
    border-radius: 18px;
    padding: 26px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}

.page-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 22px;
    flex-wrap: wrap;
}

.page-header h1 {
    margin: 0 0 8px;
    color: #2b3674;
}

.page-header p {
    margin: 0;
    color: #707eae;
}

.header-actions {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
}

.create-btn,
.assign-btn,
.primary-btn,
.secondary-btn,
.edit-btn,
.danger-btn,
.success-btn,
.page-btn {
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-weight: 600;
    transition: 0.2s;
}

.create-btn {
    background: #4caf50;
    color: white;
    padding: 10px 18px;
}

.assign-btn {
    background: #2196f3;
    color: white;
    padding: 10px 18px;
}

.toolbar {
    display: flex;
    gap: 12px;
    margin-bottom: 22px;
    flex-wrap: wrap;
}

.search-box {
    flex: 1;
    min-width: 260px;
}

.search-box input,
.filter-box select,
.form-group input,
.form-group select {
    width: 100%;
    padding: 11px 12px;
    border-radius: 8px;
    border: 1px solid #d6dce8;
    background: white;
    outline: none;
}

.search-box input:focus,
.filter-box select:focus,
.form-group input:focus,
.form-group select:focus {
    border-color: #4318ff;
    box-shadow: 0 0 0 3px rgba(67, 24, 255, 0.08);
}

.filter-box {
    min-width: 220px;
}

.table-container {
    overflow-x: auto;
    border: 1px solid #e9edf5;
    border-radius: 14px;
}

table {
    width: 100%;
    border-collapse: collapse;
    min-width: 980px;
    background: white;
}

th,
td {
    padding: 14px 16px;
    text-align: left;
    border-bottom: 1px solid #eef2f7;
}

th {
    color: #5f6fa5;
    font-weight: 700;
}

.empty-cell {
    text-align: center;
    color: #94a3b8;
    padding: 28px;
}

.status-badge {
    display: inline-block;
    padding: 5px 12px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
}

.status-badge.active {
    background: #e8f7ee;
    color: #16a34a;
}

.status-badge.inactive {
    background: #fdecec;
    color: #dc2626;
}

.action-col,
.action-cell {
    white-space: nowrap;
}

.edit-btn {
    background: #ffc107;
    color: #1f2937;
    padding: 8px 12px;
    margin-right: 8px;
}

.danger-btn {
    background: #f44336;
    color: white;
    padding: 8px 12px;
}

.success-btn {
    background: #16a34a;
    color: white;
    padding: 8px 12px;
}

.table-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 16px;
    margin-top: 18px;
    flex-wrap: wrap;
}

.footer-left {
    color: #707eae;
    font-size: 14px;
}

.pagination {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
}

.page-btn {
    min-width: 40px;
    padding: 8px 12px;
    background: #eef2ff;
    color: #2b3674;
}

.page-btn.active {
    background: #4318ff;
    color: white;
}

.page-btn:disabled {
    opacity: 0.55;
    cursor: not-allowed;
}

.modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.55);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px;
    z-index: 2000;
}

.modal-card,
.confirm-card {
    width: 720px;
    max-width: 95vw;
    background: white;
    border-radius: 20px;
    box-shadow: 0 25px 50px rgba(0, 0, 0, 0.18);
}

.confirm-card {
    width: 460px;
    padding: 24px;
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 12px;
    padding: 22px 24px 16px;
    border-bottom: 1px solid #eef2f7;
}

.modal-header h3,
.confirm-card h3 {
    margin: 0 0 6px;
    color: #2b3674;
}

.modal-header p,
.confirm-text {
    margin: 0;
    color: #707eae;
    line-height: 1.5;
}

.icon-close {
    border: none;
    background: transparent;
    font-size: 18px;
    cursor: pointer;
    color: #64748b;
}

.modal-body {
    padding: 22px 24px;
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

.modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding: 18px 24px 24px;
}

.secondary-btn {
    background: #eef2f7;
    color: #334155;
    padding: 10px 16px;
}

.primary-btn {
    background: #4318ff;
    color: white;
    padding: 10px 18px;
}

.danger-solid {
    background: #dc2626;
}

.success-solid {
    background: #16a34a;
}

@media (max-width: 768px) {
    .page-card {
        padding: 18px;
    }

    .toolbar {
        flex-direction: column;
    }

    .search-box,
    .filter-box {
        min-width: unset;
        width: 100%;
    }

    .table-footer {
        flex-direction: column;
        align-items: stretch;
    }

    .pagination {
        justify-content: flex-start;
    }

    .form-grid {
        grid-template-columns: 1fr;
    }

    .modal-card,
    .confirm-card {
        width: 100%;
    }

    .modal-footer {
        flex-direction: column-reverse;
    }

    .primary-btn,
    .secondary-btn {
        width: 100%;
    }
}
</style>