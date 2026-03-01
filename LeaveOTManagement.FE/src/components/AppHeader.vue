<template>
  <header class="header">
    <div class="breadcrumb">
      <p class="path">Pages / Dashboard</p>
      <h2 class="current">Main Dashboard</h2>
    </div>
    
    <div class="user-control">
      <div class="search-bar">
        🔍 <input type="text" placeholder="Search..." />
      </div>
      <div class="user-avatar">👤</div>
    </div>
  </header>
</template>

<style scoped>
.header {
  padding: 30px 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.path { font-size: 14px; color: #707eae; }
.current { font-size: 34px; font-weight: 700; color: #2b3674; }

.user-control {
  background: white;
  padding: 10px 20px;
  border-radius: 30px;
  display: flex;
  align-items: center;
  gap: 20px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}

.search-bar {
  background: #f4f7fe;
  padding: 8px 15px;
  border-radius: 20px;
}

.search-bar input {
  border: none;
  background: transparent;
  outline: none;
  margin-left: 5px;
}
</style>