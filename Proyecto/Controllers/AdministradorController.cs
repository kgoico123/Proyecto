using Microsoft.AspNetCore.Mvc;
using Proyecto.Models;
using Microsoft.AspNetCore.Identity;
using Proyecto.Data;
using Proyecto.shared;
using Microsoft.EntityFrameworkCore;
using Proyecto.ViewModels;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace Proyecto.Controllers
{
    [Authorize(Roles = VCG.Role_Admin)]
    public class AdministradorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _context;

        public AdministradorController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDBContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Administrador/Register
        public async Task<IActionResult> Register()
        {
            NewRegisterTypeUserVM dataResponse = new NewRegisterTypeUserVM
            {
                cursos = await _context.Cursos.ToListAsync(),
                tutores = await _context.Tutores.Include(t => t.user).ToListAsync()
            };
            return View(dataResponse);
        }

        // POST: /Administrador/Register
        [HttpPost]
        public async Task<IActionResult> Register(NewRegisterTypeUserVM userVM)
        {
            if (userVM == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                // Recargar los datos necesarios para la vista antes de devolverla
                userVM.cursos = await _context.Cursos.ToListAsync();
                userVM.tutores = await _context.Tutores.Include(t => t.user).ToListAsync();
                return View(userVM);
            }

            var tipo = userVM.tipo ?? string.Empty;

            if (userVM.User == null)
            {
                ModelState.AddModelError(string.Empty, "User data is required.");
                userVM.cursos = await _context.Cursos.ToListAsync();
                userVM.tutores = await _context.Tutores.Include(t => t.user).ToListAsync();
                return View(userVM);
            }

            // Ensure password is present (use DNI if provided, otherwise a generated temp password)
            var password = string.IsNullOrEmpty(userVM.User.Dni) ? GenerateTemporaryPassword() : userVM.User.Dni;

            var result = await _userManager.CreateAsync(userVM.User, password);
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                userVM.cursos = await _context.Cursos.ToListAsync();
                userVM.tutores = await _context.Tutores.Include(t => t.user).ToListAsync();
                return View(userVM);
            }

            // Delegate role-specific work to helpers to reduce method complexity
            if (tipo == TypesRegister.Tutor)
            {
                await HandleTutorRegistrationAsync(userVM);
            }
            else if (tipo == TypesRegister.Estudiante)
            {
                await HandleEstudianteRegistrationAsync(userVM);
            }
            else if (tipo == TypesRegister.Docente)
            {
                await HandleDocenteRegistrationAsync(userVM);
            }
            else if (tipo == TypesRegister.Administrador)
            {
                await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Admin);
            }

            return RedirectToAction("Dashboard");
        }

        // GET: /Administrador/RegisterCurso
        [HttpGet]
        public async Task<IActionResult> RegisterCurso()
        {
            NewCursoVM cursoVM = new NewCursoVM()
            {
                Curso = new Curso(),
                DocenteId = 0,
                Docentes = await _context.Docentes.Include(u => u.user).ToListAsync(),
            };
            return View(cursoVM);
        }

        // POST: /Administrador/RegisterCurso
        [HttpPost]
        public async Task<IActionResult> RegisterCurso(NewCursoVM cursoVM)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar datos necesarios para la vista
                cursoVM.Docentes = await _context.Docentes.Include(u => u.user).ToListAsync();
                return View(cursoVM);
            }
            _context.Cursos.Add(cursoVM.Curso);
            await _context.SaveChangesAsync();

            // Asignar docente si corresponde
            if (cursoVM.DocenteId > 0)
            {
                int docenteId = cursoVM.DocenteId;
                var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdDocente == docenteId);
                if (docente != null)
                {
                    docente.Curso = cursoVM.Curso;
                    _context.Docentes.Update(docente);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> DetalleCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.estudiante_Curso)
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.User = await _userManager.GetUserAsync(User);
            ViewBag.UsersCount = await _userManager.Users.CountAsync();
            ViewBag.AlumnosCount = await _context.Estudiantes.CountAsync();
            ViewBag.DocentesCount = await _context.Docentes.CountAsync();
            ViewBag.CursosCount = await _context.Cursos.CountAsync();
            return View();
        }

        public async Task<IActionResult> Cursos()
        {
            var curso = await _context.Cursos
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .Include(c => c.estudiante_Curso)
                .ToListAsync();
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.estudiante_Curso)
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return Json(new { success = false, message = "Curso no encontrado." });
            }

            if (curso.estudiante_Curso?.Any() ?? false)
            {
                return Json(new { success = false, message = "No se puede eliminar el curso porque tiene estudiantes asignados." });
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Curso eliminado correctamente." });
        }

        public async Task<IActionResult> ActualizarCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return NotFound();
            }

            // Crear el ViewModel para la vista
            var cursoVM = new NewCursoVM
            {
                Curso = curso,
                // Cargar todos los docentes para el dropdown de asignación
                Docentes = await _context.Docentes
                    .Where(d => d.CursoId == null)
                    .Include(d => d.user)
                    .ToListAsync()
            };

            return View(cursoVM);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCurso(NewCursoVM cursoVM)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar los docentes para el dropdown y devolver la vista
                cursoVM.Docentes = await _context.Docentes
                    .Where(d => d.CursoId == null)
                    .Include(d => d.user)
                    .ToListAsync();
                return View(cursoVM);
            }
            // Buscar el curso existente en la base de datos
            var cursoExist = await _context.Cursos.FindAsync(cursoVM.Curso.IdCurso);
            if (cursoExist == null)
            {
                return NotFound();
            }

            // Actualizar las propiedades del curso
            cursoExist.Nombre = cursoVM.Curso.Nombre;
            cursoExist.HorarioInicio = cursoVM.Curso.HorarioInicio;
            cursoExist.HorarioFin = cursoVM.Curso.HorarioFin;
            cursoExist.aula = cursoVM.Curso.aula;
            cursoExist.Grado = cursoVM.Curso.Grado;

            _context.Cursos.Update(cursoExist);

            if (cursoVM.DocenteId > 0)
            {
                var docente = await _context.Docentes.FindAsync(cursoVM.DocenteId);
                if (docente != null)
                {
                    docente.CursoId = cursoExist.IdCurso;
                    _context.Docentes.Update(docente);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Cursos");
        }


        public async Task<IActionResult> Usuarios()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> DetalleUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);

            UserDetailVM responseData = new UserDetailVM();

            responseData.UserId = user.Id;
            responseData.role = roles;

            if (roles.Contains(VCG.Role_Tutor))
            {
                responseData.tutor = await _context.Tutores.Include(t => t.user)
                                          .Include(t => t.Estudiantes)
                                          .ThenInclude(e => e.user)
                                          .FirstOrDefaultAsync(t => t.UserId == user.Id);

            }
            else if (roles.Contains(VCG.Role_Estudiante))
            {
                responseData.estudiante = await _context.Estudiantes
                                                  .Include(e => e.user)
                                               .Include(e => e.Tutor)
                                               .ThenInclude(t => t.user)
                                               .Include(e => e.Estudiante_Cursos)
                                               .ThenInclude(ec => ec.Curso)
                                               .ThenInclude(c => c.Docentes)
                                               .ThenInclude(d => d.user)
                                               .FirstOrDefaultAsync(e => e.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Docente))
            {
                responseData.docente = await _context.Docentes
                                            .Include(d => d.user)
                                            .Include(d => d.Curso)
                                            .ThenInclude(c => c.estudiante_Curso)
                                            .FirstOrDefaultAsync(d => d.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Admin))
            {
                responseData.Administrador = user;
            }

            return View(responseData);
        }

        public async Task<IActionResult> EliminarUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);

            UserDetailVM responseData = new UserDetailVM();

            responseData.UserId = user.Id;
            responseData.role = roles;

            if (roles.Contains(VCG.Role_Tutor))
            {
                responseData.tutor = await _context.Tutores.Include(t => t.user)
                                          .Include(t => t.Estudiantes)
                                          .ThenInclude(e => e.user)
                                          .FirstOrDefaultAsync(t => t.UserId == user.Id);

            }
            else if (roles.Contains(VCG.Role_Estudiante))
            {
                responseData.estudiante = await _context.Estudiantes
                                                  .Include(e => e.user)
                                               .Include(e => e.Tutor)
                                               .ThenInclude(t => t.user)
                                               .Include(e => e.Estudiante_Cursos)
                                               .ThenInclude(ec => ec.Curso)
                                               .ThenInclude(c => c.Docentes)
                                               .ThenInclude(d => d.user)
                                               .FirstOrDefaultAsync(e => e.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Docente))
            {
                responseData.docente = await _context.Docentes
                                            .Include(d => d.user)
                                            .Include(d => d.Curso)
                                            .ThenInclude(c => c.estudiante_Curso)
                                            .FirstOrDefaultAsync(d => d.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Admin))
            {
                responseData.Administrador = user;
            }

            return View(responseData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUser(UserDetailVM user)
        {
            if (!ModelState.IsValid)
            {
                return View("DetalleUsuario", user);
            }
            if (string.IsNullOrEmpty(user.UserId))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(user.UserId);
            if (usuario == null)
            {
                return NotFound();
            }

            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.UserId == user.UserId);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
            }

            var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(e => e.UserId == user.UserId);
            if (estudiante != null)
            {
                _context.Estudiantes.Remove(estudiante);
            }

            var tutor = await _context.Tutores.FirstOrDefaultAsync(a => a.UserId == user.UserId);
            if (tutor != null)
            {
                _context.Tutores.Remove(tutor);
            }

            await _context.SaveChangesAsync();

            var resultado = await _userManager.DeleteAsync(usuario);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Usuarios");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("DetalleUsuario", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesvincularDocente([FromBody] DesvincularDocenteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Solicitud inválida." });
            }
            try
            {
                var docente = await _context.Docentes
                    .Include(d => d.user)
                    .FirstOrDefaultAsync(d => d.IdDocente == request.DocenteId);

                if (docente == null)
                {
                    return Json(new { success = false, message = "Docente no encontrado." });
                }

                if (docente.CursoId != request.CursoId)
                {
                    return Json(new { success = false, message = "El docente no está asignado a este curso." });
                }

                docente.CursoId = null;
                _context.Docentes.Update(docente);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Docente desvinculado correctamente.",
                    docenteNombre = $"{docente.user.UserName} {docente.user.Apellido}"
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error interno del servidor." });
            }
        }

        #region Registration helpers

        private static string GenerateTemporaryPassword()
        {
            // Cryptographically secure temporary password generator.
            // Ensures at least one upper, one lower, one digit and one symbol are present.
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string symbols = "!@#$%";
            string all = upper + lower + digits + symbols;
            int length = 12;

            var chars = new char[length];

            // Guarantee inclusion of each required class
            chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            chars[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];

            // Fill the rest
            for (int i = 4; i < length; i++)
            {
                chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
            }

            // Shuffle using a cryptographically secure RNG (Fisher-Yates)
            for (int i = length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                var tmp = chars[i];
                chars[i] = chars[j];
                chars[j] = tmp;
            }

            return new string(chars);
        }

        private async Task HandleTutorRegistrationAsync(NewRegisterTypeUserVM userVM)
        {
            await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Tutor);

            var tutor = new Tutor
            {
                UserId = userVM.User.Id,
                direccion = string.Empty
            };
            _context.Tutores.Add(tutor);
            await _context.SaveChangesAsync();

            // If estudiante data provided, create child user and estudiante record
            // `userVM` is non-nullable here (validated by caller), so remove the redundant null-conditional on it.
            if (!string.IsNullOrEmpty(userVM.estudiante?.user?.UserName))
            {
                var estudianteUser = userVM.estudiante.user;
                var estudiantePassword = string.IsNullOrEmpty(estudianteUser.Dni) ? GenerateTemporaryPassword() : estudianteUser.Dni;
                var createRes = await _userManager.CreateAsync(estudianteUser, estudiantePassword);
                if (createRes.Succeeded)
                {
                    var estudiante = new Estudiante
                    {
                        UserId = estudianteUser.Id,
                        TutorId = tutor.IdTutor,
                        Grado = userVM.estudiante.Grado
                    };
                    _context.Estudiantes.Add(estudiante);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task HandleEstudianteRegistrationAsync(NewRegisterTypeUserVM userVM)
        {
            await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Estudiante);

            // If tutor info provided and no TutorId, create tutor user & record
            if (userVM.estudiante != null && userVM.estudiante.TutorId <= 0 && userVM.tutor?.user != null && !string.IsNullOrEmpty(userVM.tutor.user.UserName))
            {
                var tutorUser = userVM.tutor.user;
                var tutorPassword = string.IsNullOrEmpty(tutorUser.Dni) ? GenerateTemporaryPassword() : tutorUser.Dni;
                var createTutor = await _userManager.CreateAsync(tutorUser, tutorPassword);
                if (createTutor.Succeeded)
                {
                    await _userManager.AddToRoleAsync(tutorUser, VCG.Role_Tutor);
                    userVM.tutor.UserId = tutorUser.Id;
                    _context.Tutores.Add(userVM.tutor);
                    await _context.SaveChangesAsync();
                    userVM.estudiante.TutorId = userVM.tutor.IdTutor;
                }
            }

            // Create estudiante record
            if (userVM.estudiante != null)
            {
                userVM.estudiante.UserId = userVM.User.Id;
                _context.Estudiantes.Add(userVM.estudiante);
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleDocenteRegistrationAsync(NewRegisterTypeUserVM userVM)
        {
            await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Docente);

            if (userVM.curso != null)
            {
                if (!string.IsNullOrEmpty(userVM.curso.Nombre))
                {
                    _context.Cursos.Add(userVM.curso);
                    await _context.SaveChangesAsync();
                }
                else if (userVM.curso.IdCurso > 0)
                {
                    var cursoFound = await _context.Cursos.FirstOrDefaultAsync(c => c.IdCurso == userVM.curso.IdCurso);
                    if (cursoFound != null)
                    {
                        userVM.curso = cursoFound;
                    }
                }

                var docente = new Docente
                {
                    UserId = userVM.User.Id,
                    Curso = userVM.curso
                };
                _context.Docentes.Add(docente);
                await _context.SaveChangesAsync();
            }
        }

        #endregion
    }
}
