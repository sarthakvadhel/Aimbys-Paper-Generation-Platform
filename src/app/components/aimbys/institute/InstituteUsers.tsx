import { useState } from 'react';
import { Search, Plus, Shield, BookOpen, GraduationCap, Eye, Edit2, MoreHorizontal, ChevronLeft, ChevronRight } from 'lucide-react';
import { AimbysState } from '../../../App';

const users = [
  { id: 'USR-10001', name: 'Mrs. Sunita Sharma', email: 'admin@dlps.edu.in', role: 'Institute Admin', dept: 'Administration', status: 'active', lastLogin: '2026-05-21 14:42' },
  { id: 'USR-10002', name: 'Mr. Rajiv Kumar', email: 'rajiv.kumar@dlps.edu.in', role: 'Teacher', dept: 'Mathematics', status: 'active', lastLogin: '2026-05-21 11:20' },
  { id: 'USR-10003', name: 'Mrs. Priya Nair', email: 'priya.nair@dlps.edu.in', role: 'Teacher', dept: 'Physics', status: 'active', lastLogin: '2026-05-20 16:08' },
  { id: 'USR-10004', name: 'Dr. Sunita Roy', email: 'sunita.roy@dlps.edu.in', role: 'Teacher', dept: 'Chemistry', status: 'active', lastLogin: '2026-05-21 09:44' },
  { id: 'USR-10005', name: 'Mr. Arun Mehta', email: 'arun.mehta@dlps.edu.in', role: 'Teacher', dept: 'Computer Science', status: 'active', lastLogin: '2026-05-19 14:30' },
  { id: 'USR-10006', name: 'Ms. Ananya Sharma', email: 'ananya.sharma@dlps.edu.in', role: 'Evaluator', dept: 'English', status: 'active', lastLogin: '2026-05-21 10:15' },
  { id: 'USR-10007', name: 'Mr. Kiran Bose', email: 'kiran.bose@dlps.edu.in', role: 'Proctor', dept: 'Administration', status: 'inactive', lastLogin: '2026-05-10 08:22' },
  { id: 'USR-10008', name: 'Riya Patel (Student)', email: 'riya.patel.12a@dlps.edu.in', role: 'Student', dept: 'Class XII-A', status: 'active', lastLogin: '2026-05-21 13:55' },
  { id: 'USR-10009', name: 'Arjun Rao (Student)', email: 'arjun.rao.11b@dlps.edu.in', role: 'Student', dept: 'Class XI-B', status: 'active', lastLogin: '2026-05-21 08:40' },
  { id: 'USR-10010', name: 'Meera Joshi (Student)', email: 'meera.joshi.10c@dlps.edu.in', role: 'Student', dept: 'Class X-C', status: 'suspended', lastLogin: '2026-05-08 15:10' },
];

const roleConfig: Record<string, { icon: any; color: string; bg: string }> = {
  'Institute Admin': { icon: Shield, color: 'text-violet-700 dark:text-violet-400', bg: 'bg-violet-50 dark:bg-violet-950/30' },
  'Teacher': { icon: BookOpen, color: 'text-blue-700 dark:text-blue-400', bg: 'bg-blue-50 dark:bg-blue-950/30' },
  'Evaluator': { icon: BookOpen, color: 'text-sky-700 dark:text-sky-400', bg: 'bg-sky-50 dark:bg-sky-950/30' },
  'Proctor': { icon: Shield, color: 'text-amber-700 dark:text-amber-400', bg: 'bg-amber-50 dark:bg-amber-950/30' },
  'Student': { icon: GraduationCap, color: 'text-green-700 dark:text-green-400', bg: 'bg-green-50 dark:bg-green-950/30' },
};

const statusDot: Record<string, string> = {
  active: 'bg-green-500',
  inactive: 'bg-slate-400',
  suspended: 'bg-red-500',
};

export function InstituteUsers(_props: AimbysState) {
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const [page, setPage] = useState(1);
  const perPage = 8;

  const filtered = users.filter(u => {
    const ms = u.name.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase()) || u.dept.toLowerCase().includes(search.toLowerCase());
    const mr = roleFilter === 'all' || u.role === roleFilter;
    const mst = statusFilter === 'all' || u.status === statusFilter;
    return ms && mr && mst;
  });

  const paginated = filtered.slice((page - 1) * perPage, page * perPage);
  const totalPages = Math.max(1, Math.ceil(filtered.length / perPage));

  const roleCounts = users.reduce((acc, u) => { acc[u.role] = (acc[u.role] || 0) + 1; return acc; }, {} as Record<string, number>);

  return (
    <div className="space-y-5 pb-8">
      <div>
        <div className="text-slate-400 text-xs mb-1">Institute / Users & Roles</div>
        <div className="flex items-center justify-between">
          <h1 className="text-slate-900 dark:text-white font-bold text-xl">Users & Role Management</h1>
          <button className="flex items-center gap-1.5 px-3 py-2 text-white rounded text-sm" style={{ background: '#1d4ed8' }}>
            <Plus className="w-3.5 h-3.5" />Invite User
          </button>
        </div>
      </div>

      {/* Role Summary */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
        {[
          { role: 'Institute Admin', count: roleCounts['Institute Admin'] || 0 },
          { role: 'Teacher', count: roleCounts['Teacher'] || 0 },
          { role: 'Evaluator', count: roleCounts['Evaluator'] || 0 },
          { role: 'Proctor', count: roleCounts['Proctor'] || 0 },
          { role: 'Student', count: (roleCounts['Student'] || 0) + 28397 },
        ].map(({ role, count }) => {
          const cfg = roleConfig[role];
          const Icon = cfg.icon;
          return (
            <div key={role} className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-lg p-4">
              <div className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded text-xs font-medium mb-2 ${cfg.bg} ${cfg.color}`}>
                <Icon className="w-3 h-3" />{role}
              </div>
              <div className="text-2xl font-black text-slate-900 dark:text-white">{count.toLocaleString()}</div>
            </div>
          );
        })}
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="flex items-center gap-2 flex-1 px-3 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded">
          <Search className="w-4 h-4 text-slate-400" />
          <input value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} placeholder="Search by name, email, department..." className="bg-transparent text-slate-900 dark:text-white placeholder-slate-400 outline-none flex-1 text-sm" />
        </div>
        <select value={roleFilter} onChange={e => { setRoleFilter(e.target.value); setPage(1); }} className="px-3 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded text-slate-900 dark:text-white outline-none text-sm cursor-pointer">
          {['all', 'Institute Admin', 'Teacher', 'Evaluator', 'Proctor', 'Student'].map(r => <option key={r} value={r}>{r === 'all' ? 'All Roles' : r}</option>)}
        </select>
        <select value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setPage(1); }} className="px-3 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded text-slate-900 dark:text-white outline-none text-sm cursor-pointer">
          {['all', 'active', 'inactive', 'suspended'].map(s => <option key={s} value={s}>{s === 'all' ? 'All Status' : s.charAt(0).toUpperCase() + s.slice(1)}</option>)}
        </select>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-lg overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-slate-50 dark:bg-slate-800/50 border-b border-slate-200 dark:border-slate-800">
              <tr>
                {['User ID', 'Name', 'Email', 'Role', 'Department', 'Status', 'Last Login', 'Actions'].map(h => (
                  <th key={h} className="text-left px-4 py-3 text-slate-500 dark:text-slate-400 text-xs font-semibold uppercase tracking-wider whitespace-nowrap">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {paginated.map(user => {
                const cfg = roleConfig[user.role] || roleConfig['Student'];
                const RoleIcon = cfg.icon;
                return (
                  <tr key={user.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/30 transition-colors">
                    <td className="px-4 py-3.5 font-mono text-blue-600 dark:text-blue-400 text-xs">{user.id}</td>
                    <td className="px-4 py-3.5">
                      <div className="flex items-center gap-2.5">
                        <div className="w-7 h-7 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-600 dark:text-slate-400 text-xs font-bold flex-shrink-0">
                          {user.name.split(' ').map(n => n[0]).join('').slice(0, 2)}
                        </div>
                        <span className="text-slate-900 dark:text-slate-100 font-medium text-sm whitespace-nowrap">{user.name.replace(' (Student)', '')}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3.5 text-slate-500 dark:text-slate-400 text-sm">{user.email}</td>
                    <td className="px-4 py-3.5">
                      <span className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded text-xs font-medium ${cfg.bg} ${cfg.color}`}>
                        <RoleIcon className="w-3 h-3" />{user.role}
                      </span>
                    </td>
                    <td className="px-4 py-3.5 text-slate-600 dark:text-slate-400 text-sm">{user.dept}</td>
                    <td className="px-4 py-3.5">
                      <div className="flex items-center gap-1.5">
                        <div className={`w-1.5 h-1.5 rounded-full ${statusDot[user.status]}`} />
                        <span className="text-slate-600 dark:text-slate-400 text-sm capitalize">{user.status}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3.5 text-slate-400 dark:text-slate-500 text-xs font-mono whitespace-nowrap">{user.lastLogin}</td>
                    <td className="px-4 py-3.5">
                      <div className="flex items-center gap-1">
                        <button className="p-1.5 text-slate-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-950/20 rounded transition-all"><Eye className="w-3.5 h-3.5" /></button>
                        <button className="p-1.5 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 rounded transition-all"><Edit2 className="w-3.5 h-3.5" /></button>
                        <button className="p-1.5 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 rounded transition-all"><MoreHorizontal className="w-3.5 h-3.5" /></button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
        <div className="px-4 py-3.5 border-t border-slate-200 dark:border-slate-800 flex items-center justify-between bg-slate-50 dark:bg-slate-800/30">
          <span className="text-slate-500 dark:text-slate-400 text-sm">Showing {(page - 1) * perPage + 1}–{Math.min(page * perPage, filtered.length)} of {filtered.length} (sample)</span>
          <div className="flex items-center gap-1.5">
            <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} className="p-1.5 rounded border border-slate-200 dark:border-slate-700 text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed"><ChevronLeft className="w-4 h-4" /></button>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
              <button key={p} onClick={() => setPage(p)} className={`w-8 h-8 rounded border text-sm font-medium transition-colors ${p === page ? 'border-blue-600 bg-blue-600 text-white' : 'border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800'}`}>{p}</button>
            ))}
            <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="p-1.5 rounded border border-slate-200 dark:border-slate-700 text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed"><ChevronRight className="w-4 h-4" /></button>
          </div>
        </div>
      </div>
    </div>
  );
}
